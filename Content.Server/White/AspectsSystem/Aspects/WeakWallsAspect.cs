using System.Linq;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Server.White.Other;
using Content.Shared.Damage;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class WeakWallsAspect : AspectSystem<WeakWallsAspectComponent>
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private const float DamageMultiplier = 0.15f;

    protected override void Started(EntityUid uid, WeakWallsAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<WallMarkComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            if (!TryComp<DestructibleComponent>(ent, out var destructible) ||
                !TryComp<DamageableComponent>(ent, out var damageable))
                continue;

            _damageable.SetDamageModifierSetId(ent, null, damageable);

            var trigger = (DamageTrigger?) destructible.Thresholds
                .LastOrDefault(threshold => threshold.Trigger is DamageTrigger)?.Trigger;

            if (trigger == null)
                continue;

            trigger.Damage = (int) (trigger.Damage * DamageMultiplier);
        }
    }

}
