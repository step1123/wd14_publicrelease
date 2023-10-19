using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Server.White.Other;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Tag;
using Robust.Shared.Map;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class WindowLeakAspect : AspectSystem<WindowLeakAspectComponent>
{
    [Dependency] private readonly TagSystem _tag = default!;

    protected override void Started(EntityUid uid, WindowLeakAspectComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        HashSet<EntityCoordinates> coordinatesSet = new();

        var query = EntityQueryEnumerator<WindowMarkComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var window, out var windowXform))
        {
            var coords = windowXform.Coordinates;
            coordinatesSet.Add(coords);

            EntityManager.DeleteEntity(ent);
            EntityManager.SpawnEntity(window.ReplacementProto, coords.SnapToGrid(EntityManager));
        }

        var xformQuery = EntityQueryEnumerator<TransformComponent>();
        while (xformQuery.MoveNext(out var tileEnt, out var xform))
        {
            if (coordinatesSet.Contains(xform.Coordinates) && _tag.HasTag(tileEnt, "DeleteWithWindows"))
                EntityManager.DeleteEntity(tileEnt);
        }
    }
}
