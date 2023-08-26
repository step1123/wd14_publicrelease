using Content.Shared.Movement.Events;
using Content.Shared.Projectiles;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.White.Crossbow;

public sealed class PenetratedSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PenetratedComponent, MoveInputEvent>(OnMoveInput);
    }

    private void OnMoveInput(EntityUid uid, PenetratedComponent component, ref MoveInputEvent args)
    {
        if (component is {ProjectileUid: not null, IsPinned: true})
        {
            if (!_projectile.AttemptEmbedRemove(component.ProjectileUid.Value, uid))
                FreePenetrated(uid, component);
        }
        else if (component.ProjectileUid == null && TryComp(uid, out PhysicsComponent? physics) &&
                 physics.BodyType == BodyType.Static)
        {
            FreePenetrated(uid, component, physics);
        }
    }

    public void FreePenetrated(EntityUid uid, PenetratedComponent? penetrated = null, PhysicsComponent? physics = null)
    {
        var xform = Transform(uid);
        _transform.AttachToGridOrMap(uid, xform);

        if (Resolve(uid, ref physics, false))
        {
            _physics.SetBodyType(uid, BodyType.KinematicController, body: physics, xform: xform);
            _physics.WakeBody(uid, body: physics);
        }

        if (!Resolve(uid, ref penetrated, false))
            return;

        penetrated.ProjectileUid = null;
        penetrated.IsPinned = false;
        Dirty(penetrated);
    }
}
