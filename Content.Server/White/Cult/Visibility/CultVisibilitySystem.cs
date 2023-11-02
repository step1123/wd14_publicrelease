using Content.Server.Visible;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cult.Visibility;

public sealed class CultVisibilitySystem : EntitySystem
{
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultVisibilityComponent, ComponentInit>(OnCultistVisibilityInit);
        SubscribeLocalEvent<CultVisibilityComponent, ComponentRemove>(OnCultistVisibilityRemove);

        SubscribeLocalEvent<CultVisibleComponent, ComponentInit>(OnCultistVisibleInit);
        SubscribeLocalEvent<CultVisibleComponent, ComponentRemove>(OnCultistVisibleRemove);
    }

    private void OnCultistVisibleRemove(EntityUid uid, CultVisibleComponent component, ComponentRemove args)
    {
        if (!TryComp(uid, out VisibilityComponent? visibility))
            return;

        _visibilitySystem.SetLayer(uid, visibility, (int) VisibilityFlags.Normal);
    }

    private void OnCultistVisibleInit(EntityUid uid, CultVisibleComponent component, ComponentInit args)
    {
        var visibility = EnsureComp<VisibilityComponent>(uid);
        _visibilitySystem.SetLayer(uid, visibility, (int) VisibilityFlags.CultVisible);
    }

    private void OnCultistVisibilityRemove(EntityUid uid, CultVisibilityComponent component, ComponentRemove args)
    {
        if (!TryComp(uid, out EyeComponent? eye))
            return;

        eye.VisibilityMask &= (uint) ~VisibilityFlags.CultVisible;
    }

    private void OnCultistVisibilityInit(EntityUid uid, CultVisibilityComponent component, ComponentInit args)
    {
        if (!TryComp(uid, out EyeComponent? eye))
            return;

        eye.VisibilityMask |= (uint) VisibilityFlags.CultVisible;
    }
}
