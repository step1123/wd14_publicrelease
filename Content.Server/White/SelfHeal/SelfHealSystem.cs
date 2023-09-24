using Content.Server.Administration.Logs;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.White.SelfHeal;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.White.SelfHeal;

public sealed class SelfHealSystem: EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SelfHealComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SelfHealComponent, SelfHealEvent>(OnHealingAfterInteract);
        SubscribeLocalEvent<DamageableComponent, SelfHealDoAfterEvent>(OnDoAfter);
    }

    private void OnInit(EntityUid uid, SelfHealComponent component, ComponentInit args)
    {
        if (!_prototypeManager.TryIndex<EntityTargetActionPrototype>("SelfHealAction", out var selfheal))
            return;

        component.Action = new EntityTargetAction(selfheal);
        _actionsSystem.AddAction(uid, component.Action, uid);
    }

    private void OnHealingAfterInteract(EntityUid uid, SelfHealComponent component, SelfHealEvent args)
    {
        if (args.Handled)
            return;

        TryHeal(args.Performer, args.Target, component);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, DamageableComponent component, SelfHealDoAfterEvent args)
    {
        var dontRepeat = false;

        if (!TryComp(args.User, out SelfHealComponent? healing) || args.Target == null)
            return;

        if (args.Handled || args.Cancelled)
            return;

        if (healing.DamageContainers is not null &&
            component.DamageContainerID is not null &&
            !healing.DamageContainers.Contains(component.DamageContainerID))
        {
            return;
        }

        if (!CanHeal(args.User, args.Target.Value, healing))
            return;

        Heal(args.Target.Value, healing, args);

        // Logic to determine the whether or not to repeat the healing action
        args.Repeat = (HasDamage(component, healing) && !dontRepeat);
        if (!args.Repeat && !dontRepeat)
            _popupSystem.PopupEntity(Loc.GetString("self-heal-finished-using", ("verb", Loc.GetString("self-heal-lick")), ("name", uid)), uid, args.User);

        args.Handled = true;

    }

    private void Heal(EntityUid uid, SelfHealComponent component, SelfHealDoAfterEvent args)
    {
        var healed = _damageable.TryChangeDamage(uid, component.Damage, true, origin: args.Args.User);

        if (healed == null)
            return;

        var total = healed.Total;
        var userString = EntityManager.ToPrettyString(args.User);
        var targetString = EntityManager.ToPrettyString(uid);

        var healMessage = uid != args.User
            ? $"{userString:user} healed {targetString:target} for {total:damage} with {Loc.GetString("self-heal-lick")}"
            : $"{userString:user} healed themselves for {total:damage} with {Loc.GetString("self-heal-lick")}";
        _adminLogger.Add(LogType.Healed, $"{healMessage}");

        if (TryComp<SelfHealComponent>(args.User, out var selfHealComponent))
        {
            var audio = selfHealComponent.HealingSound;
            var audioParams = new AudioParams().WithVariation(2f).WithVolume(-5f);

            _audio.PlayPvs(audio, args.User, audioParams);
            _popupSystem.PopupEntity(Loc.GetString("self-heal-using-other", ("name", uid), ("verb", Loc.GetString("self-heal-lick"))), uid);
        }
    }

    private bool CanHeal(EntityUid user, EntityUid target, SelfHealComponent component)
    {
        if (!TryComp<DamageableComponent>(target, out var targetDamage) || !HasComp<HumanoidAppearanceComponent>(target))
            return false;

        if (user != target && !_interactionSystem.InRangeUnobstructed(user, target, popup: true))
            return false;

        if (!HasDamage(targetDamage, component))
        {
            var popup = Loc.GetString("self-heal-cant-use", ("verb", Loc.GetString("self-heal-lick")),
                ("name", target));
            _popupSystem.PopupEntity(popup, user, user);
            return false;
        }

        if (component.DisallowedClothingUser != null)
        {
            foreach (var clothing in component.DisallowedClothingUser)
            {
                if (_inventorySystem.TryGetSlotEntity(user, clothing, out var blockedClothing) &&
                    EntityManager.TryGetComponent<IngestionBlockerComponent>(blockedClothing, out var blocker) &&
                    blocker.Enabled)
                {
                    var popup = Loc.GetString("self-heal-cant-use-clothing", ("verb", Loc.GetString("self-heal-lick")), ("clothing", blockedClothing));
                    _popupSystem.PopupEntity(popup, user, user);
                    return false;
                }
            }
        }

        if (component.DisallowedClothingTarget != null)
        {
            foreach (var clothing in component.DisallowedClothingTarget)
            {
                if (_inventorySystem.TryGetSlotEntity(target, clothing, out var blockedClothing))
                {
                    var popup = Loc.GetString("self-heal-cant-use-clothing-other", ("verb", Loc.GetString("self-heal-lick")), ("name", target), ("clothing", blockedClothing));
                    _popupSystem.PopupEntity(popup, user, user);
                    return false;
                }
            }
        }

        return true;
    }

    private void TryHeal(EntityUid user, EntityUid target, SelfHealComponent component)
    {
        if (!CanHeal(user, target, component))
            return;

        var doAfterEventArgs =
            new DoAfterArgs(user, component.Delay, new SelfHealDoAfterEvent(), target, target)
            {
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
            };
        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private bool HasDamage(DamageableComponent component, SelfHealComponent healing)
    {
        var damageableDict = component.Damage.DamageDict;
        var healingDict = healing.Damage.DamageDict;
        foreach (var type in healingDict)
        {
            if (damageableDict[type.Key].Value > 0)
                return true;
        }

        return false;
    }

    public sealed class SelfHealEvent : EntityTargetActionEvent {}

}
