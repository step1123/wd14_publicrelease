using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Server.White.Other;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class ChairLeakAspect : AspectSystem<ChairLeakAspectComponent>
{
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;

    protected override void Started(EntityUid uid, ChairLeakAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<ChairMarkComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            if (TryComp(ent, out StrapComponent? strap))
                _buckle.StrapRemoveAll(strap);

            EntityManager.DeleteEntity(ent);
        }
    }

}
