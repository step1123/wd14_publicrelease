using Content.Server.Ghost.Components;
using Content.Server.Visible;
using Content.Shared.Follower;
using Content.Shared.White.Administration;
using Robust.Server.GameObjects;

namespace Content.Server.White.Administration;

public sealed class InvisibilitySystem : SharedInvisibilitySystem
{
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
    [Dependency] private readonly FollowerSystem _followerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InvisibilityComponent, ComponentStartup>(OnInvisibilityStartup);
        SubscribeLocalEvent<InvisibilityComponent, ComponentShutdown>(OnInvisibilityShutdown);
    }

    private void OnInvisibilityStartup(EntityUid uid, InvisibilityComponent component, ComponentStartup args)
    {
        if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
        {
            eye.VisibilityMask |= (uint) VisibilityFlags.Invisible;
        }
    }

    private void OnInvisibilityShutdown(EntityUid uid, InvisibilityComponent component, ComponentShutdown args)
    {
        if (EntityManager.TryGetComponent(uid, out VisibilityComponent? visibility))
        {
            _visibilitySystem.RemoveLayer(uid, visibility, (int) VisibilityFlags.Invisible);
        }

        if (EntityManager.TryGetComponent(uid, out EyeComponent? eye))
        {
            eye.VisibilityMask &= ~(uint) VisibilityFlags.Invisible;
        }
    }

    public void ToggleInvisibility(EntityUid uid, InvisibilityComponent component)
    {
        if (!EntityManager.TryGetComponent(uid, out VisibilityComponent? visibility))
            return;

        _followerSystem.StopAllFollowers(uid);

        component.Invisible = !component.Invisible;

        _visibilitySystem.SetLayer(uid, visibility,
            (int) (component.Invisible ? VisibilityFlags.Invisible :
                EntityManager.HasComponent<GhostComponent>(uid) ? VisibilityFlags.Ghost : VisibilityFlags.Normal
            ));

        RaiseNetworkEvent(new InvisibilityToggleEvent(uid, component.Invisible));
    }
}
