using System.Linq;
using Content.Server.Body.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Emp;
using Content.Server.EUI;
using Content.Server.White.Cult.UI;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Inventory;
using Content.Shared.Stacks;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Actions;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public void InitializeActions()
    {
        SubscribeLocalEvent<CultistComponent, CultTwistedConstructionActionEvent>(OnTwistedConstructionAction);
        SubscribeLocalEvent<CultistComponent, CultSummonDaggerActionEvent>(OnSummonDaggerAction);
        SubscribeLocalEvent<CultistComponent, CultShadowShacklesTargetActionEvent>(OnShadowShackles);
        SubscribeLocalEvent<CultistComponent, CultElectromagneticPulseTargetActionEvent>(OnElectromagneticPulse);
        SubscribeLocalEvent<CultistComponent, CultSummonCombatEquipmentTargetActionEvent>(OnSummonCombatEquipment);
        SubscribeLocalEvent<CultistComponent, CultConcealPresenceWorldActionEvent>(OnConcealPresence);
        SubscribeLocalEvent<CultistComponent, CultBloodRitesInstantActionEvent>(OnBloodRites);
        SubscribeLocalEvent<CultistComponent, CultTeleportTargetActionEvent>(OnTeleport);
        SubscribeLocalEvent<CultistComponent, CultStunTargetActionEvent>(OnStunTarget);
    }

    private void OnStunTarget(EntityUid uid, CultistComponent component, CultStunTargetActionEvent args)
    {
        if (args.Target == uid || !HasComp<StatusEffectsComponent>(args.Target))
            return;

        if (_stunSystem.TryStun(args.Target, TimeSpan.FromSeconds(6), true))
        {
            args.Handled = true;
        }
    }

    private void OnTeleport(EntityUid uid, CultistComponent component, CultTeleportTargetActionEvent args)
    {
        if (!TryComp<BloodstreamComponent>(args.Performer, out _) || !TryComp<ActorComponent>(uid, out var actor))
            return;

        var eui = new TeleportSpellEui(args.Performer, args.Target);
        _euiManager.OpenEui(eui, actor.PlayerSession);
        eui.StateDirty();

        args.Handled = true;
    }

    private void OnBloodRites(EntityUid uid, CultistComponent component, CultBloodRitesInstantActionEvent args)
    {
        if (!TryComp<BloodstreamComponent>(args.Performer, out var bloodstreamComponent))
            return;

        var bruteDamageGroup = _prototypeManager.Index<DamageGroupPrototype>("Brute");
        var burnDamageGroup = _prototypeManager.Index<DamageGroupPrototype>("Burn");

        var xform = Transform(uid);

        var entitiesInRange = _lookup.GetEntitiesInRange(xform.MapPosition, 1.5f);

        FixedPoint2 totalBloodAmount = 0f;

        var breakLoop = false;
        foreach (var solutionEntity in entitiesInRange.ToList())
        {
            if (breakLoop)
                break;

            if (!TryComp<PuddleComponent>(solutionEntity, out var puddleComponent))
                continue;

            if (!_solutionSystem.TryGetSolution(solutionEntity, puddleComponent.SolutionName, out var solution))
                continue;

            foreach (var solutionContent in solution.Contents.ToList())
            {
                if (solutionContent.ReagentId != "Blood")
                    continue;

                totalBloodAmount += solutionContent.Quantity;

                _bloodstreamSystem.TryModifyBloodLevel(uid, solutionContent.Quantity / 6f);
                _solutionSystem.TryRemoveReagent(solutionEntity, solution, "Blood", FixedPoint2.MaxValue);

                if (GetMissingBloodValue(bloodstreamComponent) == 0)
                {
                    breakLoop = true;
                }
            }
        }

        if (totalBloodAmount == 0f)
        {
            return;
        }

        _audio.PlayPvs("/Audio/White/Cult/enter_blood.ogg", uid, AudioParams.Default);
        _damageableSystem.TryChangeDamage(uid, new DamageSpecifier(bruteDamageGroup, -20));
        _damageableSystem.TryChangeDamage(uid, new DamageSpecifier(burnDamageGroup, -20));

        args.Handled = true;
    }

    private static FixedPoint2 GetMissingBloodValue(BloodstreamComponent bloodstreamComponent)
    {
        return bloodstreamComponent.BloodMaxVolume - bloodstreamComponent.BloodSolution.Volume;
    }

    private void OnConcealPresence(EntityUid uid, CultistComponent component, CultConcealPresenceWorldActionEvent args)
    {
        if (!TryComp<BloodstreamComponent>(args.Performer, out _))
            return;
    }

    private void OnSummonCombatEquipment(
        EntityUid uid,
        CultistComponent component,
        CultSummonCombatEquipmentTargetActionEvent args)
    {
        if (!TryComp<BloodstreamComponent>(args.Performer, out _))
            return;

        _bloodstreamSystem.TryModifyBloodLevel(uid, -20, createPuddle: false);

        var coordinates = Transform(uid).Coordinates;
        var armor = Spawn("ClothingOuterArmorCult", coordinates);
        var shoes = Spawn("ClothingShoesCult", coordinates);
        var blade = Spawn("EldritchBlade", coordinates);
        var bola = Spawn("CultBola", coordinates);

        _inventorySystem.TryUnequip(uid, "outerClothing");
        _inventorySystem.TryUnequip(uid, "shoes");

        _inventorySystem.TryEquip(uid, armor, "outerClothing", force: true);
        _inventorySystem.TryEquip(uid, shoes, "shoes", force: true);

        _handsSystem.PickupOrDrop(uid, blade);
        _handsSystem.PickupOrDrop(uid, bola);

        args.Handled = true;
    }

    private void OnElectromagneticPulse(
        EntityUid uid,
        CultistComponent component,
        CultElectromagneticPulseTargetActionEvent args)
    {
        if (!TryComp<BloodstreamComponent>(args.Performer, out _))
            return;

        _bloodstreamSystem.TryModifyBloodLevel(uid, -20, createPuddle: false);

        var xform = Transform(uid);

        _empSystem.EmpPulse(xform.MapPosition, 10, 100000, 10f);
        _bloodstreamSystem.TryModifyBloodLevel(uid, -20, createPuddle: false);

        args.Handled = true;
    }

    private void OnShadowShackles(EntityUid uid, CultistComponent component, CultShadowShacklesTargetActionEvent args)
    {
        if (!TryComp<BloodstreamComponent>(args.Performer, out _))
            return;

        _bloodstreamSystem.TryModifyBloodLevel(uid, -20, createPuddle: false);

        var cuffs = Spawn("CultistCuffs", Transform(uid).Coordinates);
        _handsSystem.TryPickupAnyHand(uid, cuffs);

        args.Handled = true;
    }

    private void OnTwistedConstructionAction(
        EntityUid uid,
        CultistComponent component,
        CultTwistedConstructionActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<BloodstreamComponent>(args.Performer, out var bloodstreamComponent))
            return;

        if (!_entityManager.TryGetComponent<StackComponent>(args.Target, out var stack))
            return;

        if (stack.StackTypeId != SteelPrototypeId)
            return;

        var transform = Transform(args.Target).Coordinates;
        var count = stack.Count;

        _entityManager.DeleteEntity(args.Target);

        var material = _entityManager.SpawnEntity(RunicMetalPrototypeId, transform);

        _bloodstreamSystem.TryModifyBloodLevel(args.Performer, -15, bloodstreamComponent, false);

        if (!_entityManager.TryGetComponent<StackComponent>(material, out var stackNew))
            return;

        stackNew.Count = count;

        _popupSystem.PopupEntity(Loc.GetString("Конвертируем сталь в руиник металл!"), args.Performer, args.Performer);
        args.Handled = true;
    }

    private void OnSummonDaggerAction(EntityUid uid, CultistComponent component, CultSummonDaggerActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<BloodstreamComponent>(args.Performer, out var bloodstreamComponent))
            return;

        var xform = Transform(args.Performer).Coordinates;
        var dagger = _entityManager.SpawnEntity(RitualDaggerPrototypeId, xform);

        _bloodstreamSystem.TryModifyBloodLevel(args.Performer, -30, bloodstreamComponent, false);
        _handsSystem.TryPickupAnyHand(args.Performer, dagger);
        args.Handled = true;
    }
}
