using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class FastAndFuriousAspect : AspectSystem<FastAndFuriousAspectComponent>
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
    }

    protected override void Started(EntityUid uid, FastAndFuriousAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<MovementSpeedModifierComponent>();
        while (query.MoveNext(out var ent, out var speedModifierComponent))
        {
            _movementSystem.ChangeBaseSpeed(ent, speedModifierComponent.BaseWalkSpeed + 2.5f, speedModifierComponent.BaseSprintSpeed + 4, speedModifierComponent.Acceleration);
        }
    }

    protected override void Ended(EntityUid uid, FastAndFuriousAspectComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<MovementSpeedModifierComponent>();
        while (query.MoveNext(out var ent, out var speedModifierComponent))
        {
            _movementSystem.ChangeBaseSpeed(ent, 2.5f, 4.5f, speedModifierComponent.Acceleration);
        }
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<FastAndFuriousAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!ev.LateJoin)
                return;

            var mob = ev.Mob;

            if (!TryComp<MovementSpeedModifierComponent>(mob, out var speedModifierComponent))
                return;

            _movementSystem.ChangeBaseSpeed(mob, speedModifierComponent.BaseWalkSpeed + 2.5f, speedModifierComponent.BaseSprintSpeed + 4, speedModifierComponent.Acceleration);
        }
    }
}
