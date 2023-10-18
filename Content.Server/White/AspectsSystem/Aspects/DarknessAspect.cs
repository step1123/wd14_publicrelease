using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Server.White.Other;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class DarknessAspect : AspectSystem<DarknessAspectComponent>
{
    protected override void Started(EntityUid uid, DarknessAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<LightMarkComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            EntityManager.DeleteEntity(ent);
        }
    }
}
