using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.White;
using Robust.Shared.Configuration;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class WeakAspect : AspectSystem<WeakAspectComponent>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    protected override void Started(EntityUid uid, WeakAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _cfg.SetCVar(WhiteCVars.DamageGetModifier, 0.5f);
    }

    protected override void Ended(EntityUid uid, WeakAspectComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        _cfg.SetCVar(WhiteCVars.DamageGetModifier, 1.0f);
    }
}
