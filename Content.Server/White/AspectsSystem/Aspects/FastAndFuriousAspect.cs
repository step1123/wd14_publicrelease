using Content.Server.Cloning;
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
        SubscribeLocalEvent<MovementSpeedModifierComponent, CloningEvent>(HandleCloning);
    }

    protected override void Started(EntityUid uid, FastAndFuriousAspectComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<MovementSpeedModifierComponent>();
        while (query.MoveNext(out var ent, out var speedModifierComponent))
        {
            _movementSystem.ChangeBaseSpeed(ent, speedModifierComponent.BaseWalkSpeed,
                speedModifierComponent.BaseSprintSpeed + 3, speedModifierComponent.Acceleration);
        }
    }

    protected override void Ended(EntityUid uid, FastAndFuriousAspectComponent component, GameRuleComponent gameRule,
        GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<MovementSpeedModifierComponent>();
        while (query.MoveNext(out var ent, out var speedModifierComponent))
        {
            _movementSystem.ChangeBaseSpeed(ent, speedModifierComponent.BaseWalkSpeed,
                speedModifierComponent.BaseSprintSpeed, speedModifierComponent.Acceleration);
        }
    }

    private void HandleCloning(EntityUid uid, MovementSpeedModifierComponent component, ref CloningEvent ev)
    {
        ModifySpeedIfActive(ev.Target);
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        if (!ev.LateJoin)
            return;

        ModifySpeedIfActive(ev.Mob);
    }

    private void ModifySpeedIfActive(EntityUid mob)
    {
        var query = EntityQueryEnumerator<FastAndFuriousAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!TryComp<MovementSpeedModifierComponent>(mob, out var speedModifierComponent))
                return;

            _movementSystem.ChangeBaseSpeed(mob, speedModifierComponent.BaseWalkSpeed,
                speedModifierComponent.BaseSprintSpeed + 3, speedModifierComponent.Acceleration);
        }
    }
}
