using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind.Components;
using Content.Server.Speech.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Robust.Shared.Random;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class RandomAccentAspect : AspectSystem<RandomAccentAspectComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
    }

    protected override void Started(EntityUid uid, RandomAccentAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<MindContainerComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            ApplyRandomAccent(ent);
        }
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<RandomAccentAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!ev.LateJoin)
                return;

            var mob = ev.Mob;

            ApplyRandomAccent(mob);
        }
    }

    #region Helpers

    private enum AccentType
    {
        Stuttering,
        Spanish,
        Slurred,
        Scrambled,
        Pirate,
        Russian,
        Anime,
        Lizard,
        Backwards
    }

    private void ApplyRandomAccent(EntityUid uid)
    {
        var allAccents = Enum.GetValues(typeof(AccentType)).Cast<AccentType>().ToList();
        var randomIndex = _random.Next(allAccents.Count);
        var selectedAccent = allAccents[randomIndex];
        ApplyAccent(uid, selectedAccent);
    }

    private void ApplyAccent(EntityUid uid, AccentType accentType)
    {
        switch (accentType)
        {
            case AccentType.Stuttering:
                EntityManager.EnsureComponent<StutteringAccentComponent>(uid);
                break;
            case AccentType.Spanish:
                EntityManager.EnsureComponent<SpanishAccentComponent>(uid);
                break;
            case AccentType.Slurred:
                EntityManager.EnsureComponent<SlurredAccentComponent>(uid);
                break;
            case AccentType.Scrambled:
                EntityManager.EnsureComponent<ScrambledAccentComponent>(uid);
                break;
            case AccentType.Pirate:
                EntityManager.EnsureComponent<PirateAccentComponent>(uid);
                break;
            case AccentType.Russian:
                EntityManager.EnsureComponent<RussianAccentComponent>(uid);
                break;
            case AccentType.Anime:
                EntityManager.EnsureComponent<OwOAccentComponent>(uid);
                break;
            case AccentType.Lizard:
                EntityManager.EnsureComponent<LizardAccentComponent>(uid);
                break;
            case AccentType.Backwards:
                EntityManager.EnsureComponent<BackwardsAccentComponent>(uid);
                break;
        }
    }

    #endregion
}
