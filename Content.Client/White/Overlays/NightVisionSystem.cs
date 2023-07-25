using Content.Shared.GameTicking;
using Content.Shared.White.Overlays;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;

namespace Content.Client.White.Overlays;

public sealed class NightVisionSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private NightVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<NightVisionComponent, ComponentRemove>(OnRemove);

        SubscribeLocalEvent<NightVisionComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<NightVisionComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, NightVisionComponent component, PlayerAttachedEvent args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid)
            return;

        _overlayMan.AddOverlay(_overlay);
        _lightManager.DrawLighting = false;
    }

    private void OnPlayerDetached(EntityUid uid, NightVisionComponent component, PlayerDetachedEvent args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid)
            return;

        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }

    private void OnInit(EntityUid uid, NightVisionComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid)
            return;

        _overlayMan.AddOverlay(_overlay);
        _lightManager.DrawLighting = false;
    }

    private void OnRemove(EntityUid uid, NightVisionComponent component, ComponentRemove args)
    {
        if (_player.LocalPlayer?.ControlledEntity != uid)
            return;

        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }
}
