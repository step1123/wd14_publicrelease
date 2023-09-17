using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Server.White.Other;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class ChairLeakAspect : AspectSystem<ChairLeakAspectComponent>
{
    protected override void Started(EntityUid uid, ChairLeakAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<ChairMarkComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            EntityManager.DeleteEntity(ent);
        }
    }

}
