using System.Numerics;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Projectiles;
using Content.Shared.Sound.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.White.Crossbow;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Projectiles
{
    public abstract class SharedProjectileSystem : EntitySystem
    {
        public const string ProjectileFixture = "projectile";

        [Dependency] private readonly INetManager _netManager = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly PenetratedSystem _penetratedSystem = default!; // WD

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ProjectileComponent, PreventCollideEvent>(PreventCollision);
            SubscribeLocalEvent<EmbeddableProjectileComponent, ProjectileCollideEvent>(OnEmbedProjectileCollide);
            SubscribeLocalEvent<EmbeddableProjectileComponent, ThrowDoHitEvent>(OnEmbedThrowDoHit);
            SubscribeLocalEvent<EmbeddableProjectileComponent, ActivateInWorldEvent>(OnEmbedActivate);
            SubscribeLocalEvent<EmbeddableProjectileComponent, RemoveEmbeddedProjectileEvent>(OnEmbedRemove);

            SubscribeLocalEvent<EmbeddableProjectileComponent, LandEvent>(OnLand); // WD
            SubscribeLocalEvent<EmbeddableProjectileComponent, ComponentRemove>(OnRemove); // WD
            SubscribeLocalEvent<EmbeddableProjectileComponent, EntityTerminatingEvent>(OnEntityTerminating); // WD
        }

        // WD EDIT START
        private void OnEntityTerminating(EntityUid uid, EmbeddableProjectileComponent component,
            ref EntityTerminatingEvent args)
        {
            FreePenetrated(component);
        }

        private void OnRemove(EntityUid uid, EmbeddableProjectileComponent component, ComponentRemove args)
        {
            FreePenetrated(component);
        }

        private void FreePenetrated(EmbeddableProjectileComponent component)
        {
            if (component.PenetratedUid == null)
                return;

            _penetratedSystem.FreePenetrated(component.PenetratedUid.Value);
            component.PenetratedUid = null;
        }

        private void OnLand(EntityUid uid, EmbeddableProjectileComponent component, ref LandEvent args)
        {
            if (component.PenetratedUid == null)
                return;

            var penetratedUid = component.PenetratedUid.Value;
            component.PenetratedUid = null;

            _penetratedSystem.FreePenetrated(penetratedUid);

            Embed(uid, penetratedUid, component);
        }

        private void OnEmbedActivate(EntityUid uid, EmbeddableProjectileComponent component, ActivateInWorldEvent args)
        {
            if (args.Handled || !TryComp<PhysicsComponent>(uid, out var physics) || physics.BodyType != BodyType.Static)
                return;

            args.Handled = true;
            AttemptEmbedRemove(uid, args.User, component);
        }

        public void AttemptEmbedRemove(EntityUid uid, EntityUid user, EmbeddableProjectileComponent? component = null)
        {
            if (!Resolve(uid, ref component, false))
                return;

            // Nuh uh
            if (component.RemovalTime == null)
                return;

            _doAfter.TryStartDoAfter(new DoAfterArgs(user, component.RemovalTime.Value,
                new RemoveEmbeddedProjectileEvent(), eventTarget: uid, target: uid)
            {
                DistanceThreshold = SharedInteractionSystem.InteractionRange,
            });
        }
        // WD EDIT END

        private void OnEmbedRemove(EntityUid uid, EmbeddableProjectileComponent component, RemoveEmbeddedProjectileEvent args)
        {
            // Whacky prediction issues.
            if (args.Cancelled || _netManager.IsClient)
                return;

            if (component.DeleteOnRemove)
            {
                QueueDel(uid);
                return;
            }

            var xform = Transform(uid);
            TryComp<PhysicsComponent>(uid, out var physics);
            _physics.SetBodyType(uid, BodyType.Dynamic, body: physics, xform: xform);
            _transform.AttachToGridOrMap(uid, xform);

            // WD START
            if (component.PenetratedUid != null)
            {
                _penetratedSystem.FreePenetrated(component.PenetratedUid.Value);
                component.PenetratedUid = null;
            }

            RaiseLocalEvent(uid, new EmbedRemovedEvent());
            // WD END

            // Land it just coz uhhh yeah
            var landEv = new LandEvent(args.User, true);
            RaiseLocalEvent(uid, ref landEv);
            _physics.WakeBody(uid, body: physics);
        }

        private void OnEmbedThrowDoHit(EntityUid uid, EmbeddableProjectileComponent component, ThrowDoHitEvent args)
        {
            // WD START
            if (component is {Penetrate: true, PenetratedUid: null} &&
                TryComp(args.Target, out PenetratedComponent? penetrated) &&
                penetrated is {ProjectileUid: null, IsPinned: false} &&
                TryComp(args.Target, out PhysicsComponent? physics))
            {
                component.PenetratedUid = args.Target;
                penetrated.ProjectileUid = uid;
                _physics.SetLinearVelocity(args.Target, Vector2.Zero, body: physics);
                _physics.SetBodyType(args.Target, BodyType.Static, body: physics);
                var xform = Transform(args.Target);
                _transform.SetLocalPosition(xform, Transform(uid).LocalPosition);
                _transform.SetParent(args.Target, xform, uid);
                if (TryComp(uid, out PhysicsComponent? projPhysics))
                    _physics.SetLinearVelocity(uid, projPhysics.LinearVelocity / 2, body: projPhysics);
                Dirty(component);
                return;
            }

            if (component.PenetratedUid == args.Target)
                args.Handled = true;
            // WD END

            Embed(uid, args.Target, component);
        }

        private void OnEmbedProjectileCollide(EntityUid uid, EmbeddableProjectileComponent component, ref ProjectileCollideEvent args)
        {
            Embed(uid, args.OtherEntity, component);

            // Raise a specific event for projectiles.
            if (TryComp<ProjectileComponent>(uid, out var projectile))
            {
                var ev = new ProjectileEmbedEvent(projectile.Shooter, projectile.Weapon, args.OtherEntity);
                RaiseLocalEvent(uid, ref ev);
            }
        }

        private void Embed(EntityUid uid, EntityUid target, EmbeddableProjectileComponent component)
        {
            if (component.PreventEmbedding || component.PenetratedUid == target) // WD START
                return;

            var ev = new EmbedStartEvent(component);
            RaiseLocalEvent(uid, ref ev);

            if (TryComp(component.PenetratedUid, out PenetratedComponent? penetrated))
                penetrated.IsPinned = true;
            // WD END

            TryComp<PhysicsComponent>(uid, out var physics);
            _physics.SetLinearVelocity(uid, Vector2.Zero, body: physics);
            _physics.SetBodyType(uid, BodyType.Static, body: physics);
            var xform = Transform(uid);
            _transform.SetParent(uid, xform, target);

            if (component.Offset != Vector2.Zero)
            {
                _transform.SetLocalPosition(xform, xform.LocalPosition + xform.LocalRotation.RotateVec(component.Offset));
            }
        }

        private void PreventCollision(EntityUid uid, ProjectileComponent component, ref PreventCollideEvent args)
        {
            if (component.IgnoreShooter && args.OtherEntity == component.Shooter)
            {
                args.Cancelled = true;
            }
        }

        public void SetShooter(ProjectileComponent component, EntityUid uid)
        {
            if (component.Shooter == uid)
                return;

            component.Shooter = uid;
            Dirty(uid, component);
        }

        [Serializable, NetSerializable]
        private sealed class RemoveEmbeddedProjectileEvent : DoAfterEvent
        {
            public override DoAfterEvent Clone() => this;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ImpactEffectEvent : EntityEventArgs
    {
        public string Prototype;
        public EntityCoordinates Coordinates;

        public ImpactEffectEvent(string prototype, EntityCoordinates coordinates)
        {
            Prototype = prototype;
            Coordinates = coordinates;
        }
    }
}

/// <summary>
/// Raised when entity is just about to be hit with projectile but can reflect it
/// </summary>
[ByRefEvent]
public record struct ProjectileReflectAttemptEvent(EntityUid ProjUid, ProjectileComponent Component, bool Cancelled);

/// <summary>
/// Raised when projectile hits other entity
/// </summary>
[ByRefEvent]
public readonly record struct ProjectileHitEvent(EntityUid Target);
