using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.Access.Components;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class AiRunLockAspect : AspectSystem<AiRunLockAspectComponent>
{
    protected override void Started(EntityUid uid, AiRunLockAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<AccessReaderComponent>();
        while (query.MoveNext(out _, out var accessReaderComponent))
        {
            accessReaderComponent.Enabled = false;
        }
    }
}
