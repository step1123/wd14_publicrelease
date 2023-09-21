using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Server.White.Snatcherprod;

public sealed class SnatcherprodSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private const string CellSlot = "cell_slot";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SnatcherprodComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<SnatcherprodComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SnatcherprodComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
        SubscribeLocalEvent<SnatcherprodComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<SnatcherprodComponent, StaminaMeleeHitEvent>(OnHit);
        SubscribeLocalEvent<SnatcherprodComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<SnatcherprodComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
    }

    private void OnEntInserted(EntityUid uid, SnatcherprodComponent component, EntInsertedIntoContainerMessage args)
    {
        component.Activated = false;
        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            _appearance.SetData(uid, ToggleVisuals.Toggled, false, appearance);
        }
    }

    private void OnEntRemoved(EntityUid uid, SnatcherprodComponent component, EntRemovedFromContainerMessage args)
    {
        if (!component.Activated)
        {
            if (TryComp<AppearanceComponent>(uid, out var appearance))
            {
                _appearance.SetData(uid, ToggleVisuals.Toggled, "nocell", appearance);
            }
        }
        else
            TurnOff(uid, component);
    }

    private void OnHit(EntityUid uid, SnatcherprodComponent component, StaminaMeleeHitEvent args)
    {
        if (!component.Activated || args.HitList.Count == 0)
            return;

        var entity = args.HitList.First().Entity;

        if (!TryComp(entity, out HandsComponent? hands))
            return;

        EntityUid? heldEntity = null;

        if (hands.ActiveHandEntity != null)
            heldEntity = hands.ActiveHandEntity;
        else
        {
            foreach (var hand in hands.Hands)
            {
                if (hand.Value.HeldEntity == null)
                    continue;

                heldEntity = hand.Value.HeldEntity;
                break;
            }

            if (heldEntity == null)
                return;
        }

        if (!_hands.TryDrop(entity, heldEntity.Value, checkActionBlocker: false, handsComp: hands))
            return;

        _hands.PickupOrDrop(args.User, heldEntity.Value, false);
    }

    private void OnGetMeleeDamage(EntityUid uid, SnatcherprodComponent component, ref GetMeleeDamageEvent args)
    {
        if (!component.Activated)
            return;

        // Don't apply damage if it's activated; just do stamina damage.
        args.Damage = new DamageSpecifier();
    }

    private void OnStaminaHitAttempt(EntityUid uid, SnatcherprodComponent component, ref StaminaDamageOnHitAttemptEvent args)
    {
        if (!component.Activated || !TryGetBatteryComponent(uid, out var battery, out var batteryUid) ||
            !_battery.TryUseCharge(batteryUid.Value, component.EnergyPerUse, battery))
        {
            args.Cancelled = true;
            return;
        }

        args.HitSoundOverride = component.StunSound;

        if (!(battery.CurrentCharge < component.EnergyPerUse))
            return;

        _audio.Play(_audio.GetSound(component.SparksSound), Filter.Pvs(uid, entityManager: EntityManager), uid, true,
            AudioHelpers.WithVariation(0.25f));
        TurnOff(uid, component);
    }

    private void OnUseInHand(EntityUid uid, SnatcherprodComponent comp, UseInHandEvent args)
    {
        if (comp.Activated)
        {
            TurnOff(uid, comp);
        }
        else
        {
            TurnOn(uid, comp, args.User);
        }
    }

    private void OnExamined(EntityUid uid, SnatcherprodComponent comp, ExaminedEvent args)
    {
        var msg = comp.Activated
            ? Loc.GetString("comp-snatcherprod-examined-on")
            : Loc.GetString("comp-snatcherprod-examined-off");
        args.PushMarkup(msg);
    }

    private void TurnOff(EntityUid uid, SnatcherprodComponent comp)
    {
        if (!comp.Activated)
            return;

        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            if (TryGetBatteryComponent(uid, out _, out _))
                _appearance.SetData(uid, ToggleVisuals.Toggled, false, appearance);
            else
                _appearance.SetData(uid, ToggleVisuals.Toggled, "nocell", appearance);
        }

        _audio.Play(_audio.GetSound(comp.SparksSound), Filter.Pvs(uid, entityManager: EntityManager), uid, true,
            AudioHelpers.WithVariation(0.25f));

        comp.Activated = false;
    }

    private void TurnOn(EntityUid uid, SnatcherprodComponent comp, EntityUid user)
    {
        if (comp.Activated)
            return;

        var playerFilter = Filter.Pvs(uid, entityManager: EntityManager);
        if (!TryGetBatteryComponent(uid, out var battery, out _) || battery.CurrentCharge < comp.EnergyPerUse)
        {
            _audio.Play(_audio.GetSound(comp.TurnOnFailSound), playerFilter, uid, true,
                AudioHelpers.WithVariation(0.25f));
            _popup.PopupEntity(Loc.GetString("stunbaton-component-low-charge"), user);
            return;
        }

        if (EntityManager.TryGetComponent<AppearanceComponent>(uid, out var appearance))
        {
            _appearance.SetData(uid, ToggleVisuals.Toggled, true, appearance);
        }

        _audio.Play(_audio.GetSound(comp.SparksSound), playerFilter, uid, true,
            AudioHelpers.WithVariation(0.25f));
        comp.Activated = true;
    }

    private bool TryGetBatteryComponent(EntityUid uid, [NotNullWhen(true)] out BatteryComponent? battery,
        [NotNullWhen(true)] out EntityUid? batteryUid)
    {
        if (!_containers.TryGetContainer(uid, CellSlot, out var container) ||
            container is not ContainerSlot slot)
        {
            battery = null;
            batteryUid = null;
            return false;
        }

        batteryUid = slot.ContainedEntity;

        if (batteryUid != null)
            return TryComp(batteryUid, out battery);

        battery = null;
        return false;
    }
}
