using Content.Client.EntityJobInfo;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.White.Mindshield;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;


namespace Content.Client.Mindshield;

public sealed class MindshieldSystem : EntitySystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private MindShieldOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShowMindShieldHudComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShowMindShieldHudComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<ShowMindShieldHudComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ShowMindShieldHudComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        _overlay = new(EntityManager);
    }

    private void OnInit(EntityUid uid, ShowMindShieldHudComponent component, ComponentInit args)
    {
        if (_playerManager.LocalPlayer?.ControlledEntity == uid)
        {
            _overlayMan.AddOverlay(_overlay);
        }
    }

    private void OnRemove(EntityUid uid, ShowMindShieldHudComponent component, ComponentRemove args)
    {
        if (_playerManager.LocalPlayer?.ControlledEntity == uid)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }

    private void OnPlayerAttached(EntityUid uid, ShowMindShieldHudComponent component, PlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, ShowMindShieldHudComponent component, PlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
