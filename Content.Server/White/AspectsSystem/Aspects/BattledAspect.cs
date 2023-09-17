using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.White.NonPeacefulRoundEnd;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class BattledAspect : AspectSystem<BattledAspectComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    private NonPeacefulRoundItemsPrototype _nonPeacefulRoundItemsPrototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
    }

    protected override void Started(EntityUid uid, BattledAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var prototypes =  _prototypeManager.EnumeratePrototypes<NonPeacefulRoundItemsPrototype>().ToList();

        if (prototypes.Count == 0)
            ForceEndSelf(uid, gameRule);

        _nonPeacefulRoundItemsPrototype = _robustRandom.Pick(prototypes);

        var query = EntityQueryEnumerator<HumanoidAppearanceComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            GiveItem(ent);
        }

    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<BattledAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!ev.LateJoin)
                return;

            var mob = ev.Mob;

            GiveItem(mob);
        }
    }

    #region Helpers

    private void GiveItem(EntityUid player)
    {
        var item = _robustRandom.Pick(_nonPeacefulRoundItemsPrototype.Items);

        var transform = CompOrNull<TransformComponent>(player);

        if(transform == null)
            return;

        if(!HasComp<HandsComponent>(player))
            return;

        var weaponEntity = EntityManager.SpawnEntity(item, transform.Coordinates);

        _handsSystem.TryDrop(player);
        _handsSystem.PickupOrDrop(player, weaponEntity);
    }

    #endregion
}
