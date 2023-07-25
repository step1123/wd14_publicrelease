using System.Linq;
using Content.Server.Visible;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Server.White.IncorporealSystem;

public sealed class IncorporealSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<IncorporealComponent, ComponentStartup>(OnComponentInit);
        SubscribeLocalEvent<IncorporealComponent, ComponentShutdown>(OnComponentRemoved);
        SubscribeLocalEvent<IncorporealComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);

    }

    private void OnComponentInit(EntityUid uid, IncorporealComponent component, ComponentStartup args)
    {
        if (TryComp<FixturesComponent>(uid, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.Values.First();

            _physics.SetCollisionMask(uid, fixture, (int) CollisionGroup.GhostImpassable, fixtures);
            _physics.SetCollisionLayer(uid, fixture, 0, fixtures);
        }

        if (TryComp<VisibilityComponent>(uid, out var visibility))
        {
            _visibilitySystem.AddLayer(uid, visibility, (int) VisibilityFlags.Ghost, false);
            _visibilitySystem.RemoveLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RefreshVisibility(uid);
        }

        _movement.RefreshMovementSpeedModifiers(uid);
    }

    private void OnComponentRemoved(EntityUid uid, IncorporealComponent component, ComponentShutdown args)
    {
        if (TryComp<FixturesComponent>(uid, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.Values.First();

            _physics.SetCollisionMask(uid, fixture, (int) (CollisionGroup.FlyingMobMask | CollisionGroup.GhostImpassable), fixtures);
            _physics.SetCollisionLayer(uid, fixture, (int) CollisionGroup.FlyingMobLayer, fixtures);
        }

        if (TryComp<VisibilityComponent>(uid, out var visibility))
        {
            _visibilitySystem.RemoveLayer(uid, visibility, (int) VisibilityFlags.Ghost, false);
            _visibilitySystem.AddLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RefreshVisibility(uid);
        }

        component.MovementSpeedBuff = 1;
        _movement.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefresh(EntityUid uid, IncorporealComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.MovementSpeedBuff, component.MovementSpeedBuff);
    }
}
