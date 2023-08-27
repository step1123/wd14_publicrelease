using System.Linq;
using Content.Shared.Humanoid;
using Content.Shared.White.Reva.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.White.Reva.Systems;

public sealed class RevolutionSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;


    private ResPath _hudPath = new("/Textures/White/Overlays/Revolution/hud.rsi");

    private ShaderInstance _shader = default!;

    private string _revolutionaryHeadState = "reva_head_animated";
    private string _revolutionaryMinionState = "reva_podsos_animated";

    private List<SpriteComponent> _huds = new();

    private bool _overlayEnabled;

    public bool OverlayEnabled
    {
        get => _overlayEnabled;
        set
        {
            _overlayEnabled = value;
            DisableHud();
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevolutionaryComponent, ComponentHandleState>(OnHandleState);
        SubscribeLocalEvent<RevolutionaryComponent, ComponentAdd>(OnComponentInit);
        SubscribeLocalEvent<RevolutionaryComponent, ComponentRemove>(OnComponentRemoved);
        SubscribeLocalEvent<PlayerAttachSysMessage>(OnMobStateChanged);


        _shader = _prototypeManager.Index<ShaderPrototype>("shaded").Instance();
    }

    private void OnMobStateChanged(PlayerAttachSysMessage ev)
    {
        var hasComp = HasComp<RevolutionaryComponent>(ev.AttachedEntity);
        OverlayEnabled = hasComp;
    }

    private void OnComponentInit(EntityUid uid, RevolutionaryComponent component, ComponentAdd args)
    {
        if(uid != _playerManager.LocalPlayer?.ControlledEntity) return;

        OverlayEnabled = true;
    }

    private void OnComponentRemoved(EntityUid uid, RevolutionaryComponent component, ComponentRemove args)
    {
        if(uid != _playerManager.LocalPlayer?.ControlledEntity) return;
        OverlayEnabled = false;
    }

    private void OnHandleState(EntityUid uid, RevolutionaryComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not RevolutionaryComponentState state)
            return;

        component.HeadRevolutionary = state.HeadRevolutionary;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        if(!_overlayEnabled) return;

        var entities = EntityManager.EntityQuery<HumanoidAppearanceComponent, RevolutionaryComponent>();
        var existingHuds = new List<SpriteComponent>();
        foreach (var (_, revolutionaryComponent) in entities)
        {
            var entity = revolutionaryComponent.Owner;
            var spriteComponent = Comp<SpriteComponent>(entity);
            existingHuds.Add(spriteComponent);
            if(_huds.Contains(spriteComponent)) continue;

            AddHud(revolutionaryComponent, spriteComponent);
        }

        var removedHuds = _huds.Except(existingHuds).ToList();

        foreach (var removedHud in removedHuds)
        {
            RemoveHud(removedHud);
        }
    }

    private void AddHud(RevolutionaryComponent revolutionaryComponent, SpriteComponent spriteComponent)
    {
        var layerExists = spriteComponent.LayerMapTryGet(RevolutionaryComponent.LayerName, out var layer);
        if (!layerExists)
            layer = spriteComponent.LayerMapReserveBlank(RevolutionaryComponent.LayerName);

        spriteComponent.LayerSetRSI(layer, _hudPath);
        spriteComponent.LayerSetShader(layer, _shader);

        if (revolutionaryComponent.HeadRevolutionary)
        {
            spriteComponent.LayerSetState(layer, _revolutionaryHeadState);
        }
        else
        {
            spriteComponent.LayerSetState(layer, _revolutionaryMinionState);

        }

        _huds.Add(spriteComponent);
    }

    private void RemoveHud(SpriteComponent spriteComponent)
    {
        if(!_huds.Contains(spriteComponent)) return;
        var layerExists = spriteComponent.LayerMapTryGet(RevolutionaryComponent.LayerName, out var layer);
        if(!layerExists) return;

        if (HasComp<TransformComponent>(spriteComponent.Owner))
        {
            spriteComponent.RemoveLayer(layer);
        }

        _huds.Remove(spriteComponent);
    }

    private void DisableHud()
    {
        foreach (var hud in _huds)
        {
            var layerExists = hud.LayerMapTryGet(RevolutionaryComponent.LayerName, out var layer);
            if(!layerExists) continue;

            hud.RemoveLayer(layer);
        }

        _huds.Clear();
    }
}
