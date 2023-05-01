using Content.Server.Chat.Systems;
using Content.Server.Ghost.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Server.White.Other;

public sealed class OnDeath : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HumanoidAppearanceComponent, MobStateChangedEvent>(HandleDeathEvent);
        SubscribeLocalEvent<GhostComponent, ComponentInit>(OnGhosted);
    }

    private readonly Dictionary<EntityUid, IPlayingAudioStream> _playingStreams = new();
    private static readonly SoundSpecifier DeathSounds = new SoundCollectionSpecifier("deathSounds");
    private static readonly SoundSpecifier HeartSounds = new SoundCollectionSpecifier("heartSounds");
    private static readonly string[] DeathGaspMessages =
    {
        "death-gasp-high",
        "death-gasp-medium",
        "death-gasp-normal"
    };

    private void HandleDeathEvent(EntityUid uid, HumanoidAppearanceComponent component, MobStateChangedEvent args)
    {
        //^.^
        switch (args.NewMobState)
        {
            case MobState.Invalid:
                StopPlayingStream(uid);
                break;
            case MobState.Alive:
                StopPlayingStream(uid);
                break;
            case MobState.Critical:
                PlayPlayingStream(uid);
                break;
            case MobState.Dead:
                StopPlayingStream(uid);
                var deathGaspMessage = SelectRandomDeathGaspMessage();
                var localizedMessage = LocalizeDeathGaspMessage(deathGaspMessage);
                SendDeathGaspMessage(uid, localizedMessage);
                PlayDeathSound(uid);
                break;
        }
    }


    private void PlayPlayingStream(EntityUid uid)
    {
        if (_playingStreams.TryGetValue(uid, out var currentStream))
        {
            currentStream.Stop();
        }

        var newStream = _audio.PlayEntity(HeartSounds, uid, uid, AudioParams.Default.WithLoop(true));
        if (newStream != null)
        {
            _playingStreams[uid] = newStream;
        }

    }

    private void StopPlayingStream(EntityUid uid)
    {
        if (_playingStreams.TryGetValue(uid, out var currentStream))
        {
            currentStream.Stop();
            _playingStreams.Remove(uid);
        }
    }

    private string SelectRandomDeathGaspMessage()
        => DeathGaspMessages[_random.Next(DeathGaspMessages.Length)];

    private string LocalizeDeathGaspMessage(string message)
        => Loc.GetString(message);

    private void SendDeathGaspMessage(EntityUid uid, string message)
        => _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Emote, false, force: true);

    private void PlayDeathSound(EntityUid uid)
        => _audio.PlayEntity(DeathSounds, uid, uid, AudioParams.Default);

    private void OnGhosted(EntityUid uid, GhostComponent component, ComponentInit args)
    {
        StopPlayingStream(uid);
    }

}
