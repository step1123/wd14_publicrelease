using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.White;
using Robust.Shared.Configuration;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class SlipperyAspect : AspectSystem<SlipperyAspectComponent>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    protected override void Started(EntityUid uid, SlipperyAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _cfg.SetCVar(WhiteCVars.SlipPowerModifier, 2f);
    }

    protected override void Ended(EntityUid uid, SlipperyAspectComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        _cfg.SetCVar(WhiteCVars.SlipPowerModifier, 1f);
    }
}
