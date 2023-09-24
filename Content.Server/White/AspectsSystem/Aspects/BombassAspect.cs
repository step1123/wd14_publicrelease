using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Robust.Shared.Random;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class BombassAspect : AspectSystem<BombassAspectComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Added(EntityUid uid, BombassAspectComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        SpawnMines();
    }

    private void SpawnMines()
    {
        var minMines = _random.Next(40, 60);

        for (var i = 0; i < minMines; i++)
        {
            if (!TryFindRandomTile(out _, out _, out _, out var targetCoords))
                break;

            EntityManager.SpawnEntity("LandMineAspectExplosive", targetCoords);
        }
    }
}
