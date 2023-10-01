using Content.Shared.GameTicking;
using Content.Shared.White.Overlays;
using Robust.Client.Player;
using Robust.Client.Graphics;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Overlays;

public sealed class IgnoreHumanoidsSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private IgnoreHumanoidsOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgnoreHumanoidsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<IgnoreHumanoidsComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<IgnoreHumanoidsComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<IgnoreHumanoidsComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        _overlay = new IgnoreHumanoidsOverlay(EntityManager);
    }

    private void OnInit(EntityUid uid, IgnoreHumanoidsComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _overlayMan.AddOverlay(_overlay);
        }
    }

    private void OnRemove(EntityUid uid, IgnoreHumanoidsComponent component, ComponentRemove args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _overlay.Reset();
            _overlayMan.RemoveOverlay(_overlay);
        }
    }

    private void OnPlayerAttached(EntityUid uid, IgnoreHumanoidsComponent component, PlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, IgnoreHumanoidsComponent component, PlayerDetachedEvent args)
    {
        _overlay.Reset();
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _overlay.Reset();
        _overlayMan.RemoveOverlay(_overlay);
    }
}
