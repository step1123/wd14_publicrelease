using System.Linq;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Server.White.Other;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class WeakWallsAspect : AspectSystem<WeakWallsAspectComponent>
{
    private const int DamageToSet = 100;

    protected override void Started(EntityUid uid, WeakWallsAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<WallMarkComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            if (!TryComp<DestructibleComponent>(ent, out var destructible))
                continue;

            var trigger = (DamageTrigger?) destructible.Thresholds
                .LastOrDefault(threshold => threshold.Trigger is DamageTrigger)?.Trigger;

            if (trigger == null)
                continue;

            trigger.Damage = (trigger.Damage == DamageToSet) ? trigger.Damage : DamageToSet;
        }
    }

}
