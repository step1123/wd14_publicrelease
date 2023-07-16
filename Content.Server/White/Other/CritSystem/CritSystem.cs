using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Components;
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CritComponent, MeleeHitEvent>(HandleHit);
    }

    private void HandleHit(EntityUid uid, CritComponent component, MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            if (!IsCriticalHit(component))
                return;

            if (!TryComp<MobStateComponent>(target, out _))
                continue;

            var damage = args.BaseDamage.Total * component.CritMultiplier;
            var ohio = _random.Next(1, 20);

            args.BonusDamage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Blunt"), damage);

            _popup.PopupEntity($@"Crit! {damage}", args.User, PopupType.MediumCaution);

            _bloodstream.TryModifyBloodLevel(target, -ohio);
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

