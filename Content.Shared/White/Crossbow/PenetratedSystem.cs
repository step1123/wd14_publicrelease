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
            _projectile.AttemptEmbedRemove(component.ProjectileUid.Value, uid);
    }

    public void FreePenetrated(EntityUid uid, PenetratedComponent? penetrated = null)
    {
        if (!Resolve(uid, ref penetrated, false))
            return;

        var xform = Transform(uid);
        TryComp<PhysicsComponent>(uid, out var physics);
        _physics.SetBodyType(uid, BodyType.Dynamic, body: physics, xform: xform);
        _transform.AttachToGridOrMap(uid, xform);
        penetrated.ProjectileUid = null;
        penetrated.IsPinned = false;
        _physics.WakeBody(uid, body: physics);
    }
}
