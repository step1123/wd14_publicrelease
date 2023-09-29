using Content.Server.Doors.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Stunnable;
using Content.Shared.White.Cult;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Server.White.Cult.Structures;

public sealed class RunicDoorSystem : EntitySystem
{
    [Dependency] private readonly DoorSystem _doorSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RunicDoorComponent, ActivateInWorldEvent>(HandleActivate);
        SubscribeLocalEvent<RunicDoorComponent, InteractHandEvent>(HandleInteractHand);
        SubscribeLocalEvent<RunicDoorComponent, InteractUsingEvent>(HandleInteract);
        SubscribeLocalEvent<RunicDoorComponent, StartCollideEvent>(HandleCollide);
    }

    private void HandleActivate(EntityUid ent, RunicDoorComponent comp, ActivateInWorldEvent ev)
    {
        if (ev.Handled)
            return;

        Process(ent, ev.User);
    }

    private void HandleInteractHand(EntityUid ent, RunicDoorComponent comp, InteractHandEvent ev)
    {
        if (ev.Handled)
            return;

        Process(ent, ev.User);
    }

    private void HandleInteract(EntityUid ent, RunicDoorComponent comp, InteractUsingEvent ev)
    {
        if (ev.Handled)
            return;

        Process(ent, ev.User);
    }

    private void HandleCollide(EntityUid ent, RunicDoorComponent comp, ref StartCollideEvent ev)
    {
        Process(ent, ev.OtherEntity);
    }

    private void Process(EntityUid airlock, EntityUid target)
    {
        if (!this.IsPowered(airlock, EntityManager))
            return;

        if (HasComp<CultistComponent>(target))
        {
            _doorSystem.TryToggleDoor(airlock);
        }
        else
        {
            _doorSystem.Deny(airlock);

            if (!HasComp<HumanoidAppearanceComponent>(target))
                return;

            var direction = Transform(target).MapPosition.Position - Transform(airlock).MapPosition.Position;
            var impulseVector = direction * 7000;

            _physics.ApplyLinearImpulse(target, impulseVector);

            _stunSystem.TryParalyze(target, TimeSpan.FromSeconds(3), true);
        }
    }
}
