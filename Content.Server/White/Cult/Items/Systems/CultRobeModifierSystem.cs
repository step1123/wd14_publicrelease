using Content.Server.White.Cult.Items.Components;
using Content.Shared.Damage;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.White.Cult;

namespace Content.Server.White.Cult.Items.Systems;

public sealed class CultRobeModifierSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultRobeModifierComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<CultRobeModifierComponent, GotUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, CultRobeModifierComponent component, GotEquippedEvent args)
    {
        if (!HasComp<CultistComponent>(args.Equipee))
            return;

        if (args.Slot != "outerClothing")
            return;

        ModifySpeed(args.Equipee, component, true);
        ModifyDamage(args.Equipee, component, true);
    }

    private void OnUnequip(EntityUid uid, CultRobeModifierComponent component, GotUnequippedEvent args)
    {
        if (!HasComp<CultistComponent>(args.Equipee))
            return;

        if (args.Slot != "outerClothing")
            return;

        ModifySpeed(args.Equipee, component, false);
        ModifyDamage(args.Equipee, component, false);
    }

    private void ModifySpeed(EntityUid uid, CultRobeModifierComponent comp, bool increase)
    {
        if (!TryComp<MovementSpeedModifierComponent>(uid, out var move))
            return;

        var walkSpeed = increase ? move.BaseWalkSpeed * comp.SpeedModifier : move.BaseWalkSpeed / comp.SpeedModifier;

        var sprintSpeed =
            increase ? move.BaseSprintSpeed * comp.SpeedModifier : move.BaseSprintSpeed / comp.SpeedModifier;

        _movement.ChangeBaseSpeed(uid, walkSpeed, sprintSpeed, move.Acceleration, move);
    }

    private void ModifyDamage(EntityUid uid, CultRobeModifierComponent comp, bool increase)
    {
        var damageSet = string.Empty;
        if (increase)
        {
            if (!TryComp<DamageableComponent>(uid, out var damage))
                return;

            comp.StoredDamageSetId = damage.DamageModifierSetId;
            damageSet = comp.DamageModifierSetId;
        }
        else
        {
            if (comp.StoredDamageSetId != null)
                damageSet = comp.StoredDamageSetId;

            comp.StoredDamageSetId = null;
        }

        _damageable.SetDamageModifierSetId(uid, damageSet);
    }
}
