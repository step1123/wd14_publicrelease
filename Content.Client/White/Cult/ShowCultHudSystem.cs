using Content.Shared.EntityJobInfo;
using Content.Shared.White.Cult;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Cult;

public sealed class ShowCultHudSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private Overlay _overlay = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CultistComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CultistComponent, ComponentRemove>(OnComponentRemoved);
        SubscribeLocalEvent<CultistComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CultistComponent, PlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new CultHudOverlay(EntityManager, _prototypeManager);
    }

    private void OnComponentInit(EntityUid uid, CultistComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid) return;
        _overlayManager.AddOverlay(_overlay);

    }

    private void OnComponentRemoved(EntityUid uid, CultistComponent component, ComponentRemove args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid) return;
        _overlayManager.RemoveOverlay(_overlay);

    }

    private void OnPlayerAttached(EntityUid uid, CultistComponent component, PlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, CultistComponent component, PlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }
}
