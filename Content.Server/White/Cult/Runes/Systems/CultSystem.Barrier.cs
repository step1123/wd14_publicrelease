using Content.Server.White.Cult.Runes.Comps;
using Content.Shared.Interaction;
using Content.Shared.Stealth.Components;
using Content.Shared.White.Cult;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem
{
    public void InitializeBarrierSystem()
    {
        SubscribeLocalEvent<CultBarrierComponent, ActivateInWorldEvent>(OnActivateBarrier);
        SubscribeLocalEvent<CultBarrierComponent, InteractUsingEvent>(OnInteract);
    }

    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;


    private void OnActivateBarrier(EntityUid uid, CultBarrierComponent component, ActivateInWorldEvent args)
    {

        if (!HasComp<CultistComponent>(args.User))
            return;

        if (component.Activated)
            Deactivate(args.Target);
        else
            Activate(args.Target);

    }

    private void Activate(EntityUid barrier)
    {
        if (!TryComp<CultBarrierComponent>(barrier, out var barrierComponent))
            return;

        if (HasComp<StealthOnMoveComponent>(barrier))
            RemComp<StealthOnMoveComponent>(barrier);

        if (HasComp<StealthComponent>(barrier))
            RemComp<StealthComponent>(barrier);

        _physicsSystem.SetCanCollide(barrier, true);

        barrierComponent.Activated = true;
    }

    private void Deactivate(EntityUid barrier)
    {
        if (!TryComp<CultBarrierComponent>(barrier, out var barrierComponent))
            return;

        EnsureComp<StealthComponent>(barrier);
        EnsureComp<StealthOnMoveComponent>(barrier);

        _physicsSystem.SetCanCollide(barrier, false);

        barrierComponent.Activated = false;
    }

    private void OnInteract(EntityUid uid, CultBarrierComponent component, InteractUsingEvent args)
    {
        var entityPrototype = _entityManager.GetComponent<MetaDataComponent>(args.Used).EntityPrototype;

        if (entityPrototype == null)
            return;

        var used = entityPrototype.ID;
        var user = args.User;
        var target = args.Target;

        if (used != RitualDaggerPrototypeId)
            return;

        if (!HasComp<CultistComponent>(user))
            return;

        _popupSystem.PopupEntity("Вы уничтожаете барьер", user, user);

        _entityManager.DeleteEntity(target);
    }
}
