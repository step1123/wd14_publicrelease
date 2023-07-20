using System.Resources;
using Content.Shared.GameTicking;
using Content.Shared.Interaction.Events;
using Content.Shared.Physics;
using Content.Shared.White;
using Content.Shared.White.Jukebox;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Physics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Client.White.Jukebox;

public sealed class JukeboxSystem : EntitySystem
{

    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IResourceCache _resource = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IClydeAudio _clydeAudio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;



    private readonly Dictionary<JukeboxComponent, JukeboxAudio> _playingJukeboxes = new();

    private float _maxAudioRange;
    private float _jukeboxVolume;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(WhiteCVars.MaxJukeboxSoundRange, range => _maxAudioRange = range, true);
        _cfg.OnValueChanged(WhiteCVars.JukeboxVolume, JukeboxVolumeChanged, true);

        SubscribeLocalEvent<JukeboxComponent, ComponentHandleState>(OnStateChanged);
        SubscribeLocalEvent<JukeboxComponent, ComponentRemove>(OnComponentRemoved);
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeNetworkEvent<TickerJoinLobbyEvent>(JoinLobby);
        SubscribeNetworkEvent<JukeboxStopPlaying>(OnStopPlaying);
    }

    private void JukeboxVolumeChanged(float volume)
    {
        _jukeboxVolume = volume;
        CleanUp();
    }

    private void JoinLobby(TickerJoinLobbyEvent ev)
    {
        CleanUp();
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        CleanUp();
    }

    private void OnComponentRemoved(EntityUid uid, JukeboxComponent component, ComponentRemove args)
    {
        if (!_playingJukeboxes.TryGetValue(component, out var playingData)) return;
        playingData.PlayingStream.StopPlaying();
        _playingJukeboxes.Remove(component);
    }

    private void OnStopPlaying(JukeboxStopPlaying ev)
    {
        if (!ev.JukeboxUid.HasValue) return;
        if(!TryComp<JukeboxComponent>(ev.JukeboxUid, out var jukeboxComponent)) return;

        if(!_playingJukeboxes.TryGetValue(jukeboxComponent, out var jukeboxAudio)) return;

        jukeboxAudio.PlayingStream.StopPlaying();
        _playingJukeboxes.Remove(jukeboxComponent);
    }

    public void RequestSongToPlay(JukeboxComponent component, JukeboxSong jukeboxSong)
    {
        if (!_resource.TryGetResource<AudioResource>((ResPath) jukeboxSong.SongPath!, out var songResource))
        {
            return;
        }

        RaiseNetworkEvent(new JukeboxRequestSongPlay()
        {
            Jukebox = component.Owner,
            SongName = jukeboxSong.SongName,
            SongPath = jukeboxSong.SongPath,
            SongDuration = (float)songResource.AudioStream.Length.TotalSeconds
        });

    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var localPlayerEntity = _playerManager.LocalPlayer!.ControlledEntity;
        if (!localPlayerEntity.HasValue)
        {
            CleanUp();
            return;
        }

        ProcessJukeboxes();
    }

    private void ProcessJukeboxes()
    {
        var jukeboxes = EntityQuery<JukeboxComponent, TransformComponent>();
        var playerXform = Comp<TransformComponent>(_playerManager.LocalPlayer!.ControlledEntity!.Value);

        foreach (var (jukeboxComponent, jukeboxXform) in jukeboxes)
        {

            if (jukeboxXform.MapID != playerXform.MapID || (jukeboxXform.MapPosition.Position - playerXform.MapPosition.Position).Length > _maxAudioRange)
            {
                if (_playingJukeboxes.TryGetValue(jukeboxComponent, out var stream))
                {
                    _playingJukeboxes.Remove(jukeboxComponent);

                    stream.PlayingStream.StopPlaying();
                    stream.PlayingStream.Dispose();
                }

                continue;
            }

            if (_playingJukeboxes.TryGetValue(jukeboxComponent, out var jukeboxAudio))
            {
                if (!jukeboxAudio.PlayingStream.IsPlaying)
                {
                    HandleDoneStream(jukeboxAudio, jukeboxComponent, jukeboxXform, playerXform);
                    continue;
                }

                if (jukeboxAudio.SongData.SongPath != jukeboxComponent.PlayingSongData?.SongPath)
                {
                    HandleSongChanged(jukeboxAudio, jukeboxComponent, jukeboxXform, playerXform);
                    continue;
                }

                SetOcclusion(playerXform, jukeboxXform, jukeboxAudio);
                SetPosition(jukeboxXform, jukeboxAudio);
            }
            else
            {
                if (jukeboxComponent.PlayingSongData == null)
                {
                    SetBarsLayerVisible(jukeboxComponent, false);
                    continue;
                }

                var stream = TryCreateStream(jukeboxComponent, jukeboxXform, playerXform);

                if (stream == null)
                {
                    continue;
                }

                _playingJukeboxes.Add(jukeboxComponent, stream);
                SetBarsLayerVisible(jukeboxComponent, true);
            }
        }
    }

    private void SetPosition(TransformComponent jukeboxXform, JukeboxAudio jukeboxAudio)
    {
        jukeboxAudio.PlayingStream.SetPosition(jukeboxXform.MapPosition.Position);
    }

    private void SetOcclusion(TransformComponent playerXform, TransformComponent jukeboxXform, JukeboxAudio jukeboxAudio)
    {
        var collisionMask = CollisionGroup.Impassable;
        var sourceRelative = playerXform.MapPosition.Position - jukeboxXform.MapPosition.Position;
        var occlusion = 0f;

        if (sourceRelative.Length > 0)
        {
            occlusion = _physicsSystem.IntersectRayPenetration(jukeboxXform.MapID,
                new CollisionRay(jukeboxXform.MapPosition.Position, sourceRelative.Normalized, (int)collisionMask),
                sourceRelative.Length, jukeboxXform.Owner) * 3f;
        }

        jukeboxAudio.PlayingStream.SetOcclusion(occlusion);
    }

    private void HandleSongChanged(JukeboxAudio jukeboxAudio, JukeboxComponent jukeboxComponent, TransformComponent jukeboxXform, TransformComponent playerXform)
    {
        jukeboxAudio.PlayingStream.StopPlaying();

        if (jukeboxComponent.PlayingSongData != null && jukeboxComponent.PlayingSongData.SongPath == jukeboxAudio.SongData.SongPath)
        {
            var newStream = TryCreateStream(jukeboxComponent, jukeboxXform, playerXform);
            if(newStream == null) return;

            _playingJukeboxes[jukeboxComponent] = newStream;
            SetBarsLayerVisible(jukeboxComponent, true);
        }
        else
        {
            _playingJukeboxes.Remove(jukeboxComponent);
            SetBarsLayerVisible(jukeboxComponent, false);
        }
    }

    private void HandleDoneStream(JukeboxAudio jukeboxAudio, JukeboxComponent jukeboxComponent, TransformComponent jukeboxXform, TransformComponent playerXform)
    {
        if (!jukeboxComponent.Repeating)
        {
            jukeboxAudio.PlayingStream.StopPlaying();
            _playingJukeboxes.Remove(jukeboxComponent);
            SetBarsLayerVisible(jukeboxComponent, false);
            return;
        }

        if(jukeboxComponent.PlayingSongData == null) return;


        var newStream = TryCreateStream(jukeboxComponent, jukeboxXform, playerXform);

        if (newStream == null)
        {
            _playingJukeboxes.Remove(jukeboxComponent);
            SetBarsLayerVisible(jukeboxComponent, false);
        }
        else
        {

            _playingJukeboxes[jukeboxComponent] = newStream;
            SetBarsLayerVisible(jukeboxComponent, true);
        }
    }

    private JukeboxAudio? TryCreateStream(JukeboxComponent jukeboxComponent, TransformComponent jukeboxXform, TransformComponent playerXform)
    {
        if (jukeboxComponent.PlayingSongData == null) return null!;

        var resourcePath = jukeboxComponent.PlayingSongData.SongPath!;

        if(!_resource.TryGetResource<AudioResource>((ResPath) resourcePath, out var audio))
            return null!;

        if (audio.AudioStream.Length.TotalSeconds < jukeboxComponent.PlayingSongData!.PlaybackPosition)
        {
            return null!;
        }


        var playingStream = _clydeAudio.CreateAudioSource(audio.AudioStream);

        if (playingStream == null)
            return null!;

        playingStream.SetVolume(_jukeboxVolume);
        playingStream.SetRolloffFactor(3.5f);
        playingStream.SetPlaybackPosition(jukeboxComponent.PlayingSongData.PlaybackPosition);

        if (!playingStream.SetPosition(jukeboxXform.MapPosition.Position))
        {
            return null!;
        }

        var jukeboxAudio = new JukeboxAudio(playingStream!, audio, jukeboxComponent.PlayingSongData);

        SetOcclusion(playerXform, jukeboxXform, jukeboxAudio);
        playingStream.StartPlaying();

        return jukeboxAudio;
    }

    private void SetBarsLayerVisible(JukeboxComponent jukeboxComponent, bool visible)
    {
        var spriteComponent = Comp<SpriteComponent>(jukeboxComponent.Owner);
        spriteComponent.LayerMapTryGet("bars", out var layer);
        spriteComponent.LayerSetVisible(layer, visible);
    }

    private void OnStateChanged(EntityUid uid, JukeboxComponent component, ref ComponentHandleState args)
    {
        if (args.Current is JukeboxComponentState state)
        {
            component.Repeating = state.Playing;
            component.Volume = state.Volume;
            component.PlayingSongData = state.SongData;
        }
    }

    private class JukeboxAudio
    {
        public PlayingSongData SongData { get; }
        public IClydeAudioSource PlayingStream { get; }
        public AudioResource AudioStream { get; }

        public JukeboxAudio(IClydeAudioSource playingStream, AudioResource audioStream, PlayingSongData songData)
        {
            PlayingStream = playingStream;
            AudioStream = audioStream;
            SongData = songData;
        }
    }

    private void CleanUp()
    {
        foreach (var playingJukebox in _playingJukeboxes.Values)
        {
            playingJukebox.PlayingStream.StopPlaying();
            playingJukebox.PlayingStream.Dispose();
        }

        _playingJukeboxes.Clear();
    }
}
