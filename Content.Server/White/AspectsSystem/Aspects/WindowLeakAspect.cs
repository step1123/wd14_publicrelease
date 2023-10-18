using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Server.White.Other;
using Content.Shared.Coordinates.Helpers;
using Robust.Shared.Map;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class WindowLeakAspect : AspectSystem<WindowLeakAspectComponent>
{
    protected override void Started(EntityUid uid, WindowLeakAspectComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        HashSet<EntityCoordinates> coordinatesSet = new();

        var query = EntityQuery<WindowMarkComponent, TransformComponent>();
        foreach (var (window, windowXform) in query)
        {
            var coords = windowXform.Coordinates;
            coordinatesSet.Add(coords);

            var wall = EntityManager.SpawnEntity(window.ReplacementProto, coords.SnapToGrid(EntityManager));
            EnsureComp<WallMarkComponent>(wall);
        }

        var xformQuery = EntityQueryEnumerator<TransformComponent>();
        while (xformQuery.MoveNext(out var tileEnt, out var xform))
        {
            if (HasComp<WallMarkComponent>(tileEnt))
                continue;

            if (coordinatesSet.Contains(xform.Coordinates))
                EntityManager.DeleteEntity(tileEnt);
        }
    }
}
