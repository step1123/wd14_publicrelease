using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.White.Other.CritSystem;

public sealed class CritSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CritComponent, MeleeHitEvent>(HandleHit);
        SubscribeLocalEvent<CritComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid uid, CritComponent component, ExaminedEvent args)
    {
        if (component.IsBloodDagger)
        {
            args.PushMarkup(
                "[color=red]Критическая жажда: Кинжал Жажды обладает смертоносной точностью. Его владелец имеет 20% шанс нанести критический урон, поражая врага в его самые уязвимые места.\n" +
                "Кровавый абсорб: При каждом успешном критическом ударе, кинжал извлекает кровь из цели, восстанавливая здоровье владельцу пропорционально количеству высосанной крови.[/color]"
            );
        }
    }

    private void HandleHit(EntityUid uid, CritComponent component, MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            if (!IsCriticalHit(component))
                return;

            if (!TryComp<MobStateComponent>(target, out var mobState) || _mobState.IsDead(target, mobState))
                continue;

            var damage = args.BaseDamage.Total * component.CritMultiplier;

            if (component.IsBloodDagger)
            {
                var ohio = _random.Next(1, 20);
                var damageGroup = _prototypeManager.Index<DamageGroupPrototype>("Brute");

                _bloodstream.TryModifyBloodLevel(target, -ohio);
                _bloodstream.TryModifyBloodLevel(args.User, ohio);
                _damageableSystem.TryChangeDamage(args.User, new DamageSpecifier(damageGroup, -ohio * 2));

                damage = args.BaseDamage.Total * component.CritMultiplier + ohio;
            }

            args.BonusDamage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Slash"),
                damage - args.BaseDamage.Total);

            _popup.PopupEntity($@"Crit! {damage}", args.User, PopupType.MediumCaution);
        }
    }

    private bool IsCriticalHit(CritComponent component)
    {
        var roll = _random.Next(1, 101);

        var critChance = component.CritChance;

        component.WorkingChance ??= component.CritChance;

        var isCritical = roll <= component.WorkingChance;

        if (isCritical)
            component.WorkingChance = critChance;
        else
            component.WorkingChance++;

        return isCritical;
    }
}

