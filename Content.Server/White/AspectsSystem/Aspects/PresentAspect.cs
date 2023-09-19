using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Robust.Shared.Random;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class PresentAspect : AspectSystem<PresentAspectComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Added(EntityUid uid, PresentAspectComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        SpawnPresents();
    }

    private void SpawnPresents()
    {
        var minPresents = _random.Next(150, 200);

        for (var i = 0; i < minPresents; i++)
        {
            if (!TryFindRandomTile(out _, out _, out _, out var targetCoords))
                break;

            EntityManager.SpawnEntity("PresentRandomUnsafe", targetCoords);
        }
    }
}
