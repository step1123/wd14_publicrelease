using Content.Server.GameTicking.Rules.Components;
using Content.Server.Maps;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class StolenFloorAspect : AspectSystem<StolenFloorAspectComponent>
{
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly TileSystem _tileSystem = default!;

    protected override void Started(EntityUid uid, StolenFloorAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetStationGrids(out _, out var grids))
            return;

        foreach (var grid in grids)
        {
            foreach (var tile in Comp<MapGridComponent>(grid).GetAllTiles())
            {
                var tileDef = (ContentTileDefinition) _tileDefinitionManager[tile.Tile.TypeId];

                if (!tileDef.CanCrowbar)
                    continue;

                _tileSystem.DeconstructTile(tile, false);
            }
        }
    }
}
