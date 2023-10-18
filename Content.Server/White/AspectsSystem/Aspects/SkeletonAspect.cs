using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Systems;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.Humanoid;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class SkeletonAspect : AspectSystem<SkeletonAspectComponent>
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;

    private PolymorphPrototype _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);

        _proto = _protoMan.Index<PolymorphPrototype>("AspectForcedSkeleton");
    }

    protected override void Started(EntityUid uid, SkeletonAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        Dictionary<EntityUid, HumanoidAppearanceComponent> entitiesToPolymorph = new();

        var query = EntityQueryEnumerator<HumanoidAppearanceComponent>();
        while (query.MoveNext(out var ent, out var humanoid))
        {
            entitiesToPolymorph[ent] = humanoid;
        }

        foreach (var ent in entitiesToPolymorph)
        {
            PolymorphEntity(ent.Key, ent.Value);
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

            PolymorphEntity(ev.Mob);
        }
    }

    private void PolymorphEntity(EntityUid uid, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid, false))
            return;

        _humanoidAppearance.SetSpecies(uid, "Skeleton", false, humanoid);
        _humanoidAppearance.SetBodyType(uid, "SkeletonNormal", false, humanoid);
        _polymorph.PolymorphEntity(uid, _proto);
    }
}
