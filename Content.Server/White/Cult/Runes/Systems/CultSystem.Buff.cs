using System.Linq;
using Content.Server.White.Cult.GameRule;
using Content.Server.White.Cult.Runes.Comps;
using Content.Shared.Alert;
using Content.Shared.Maps;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Components;
using Robust.Shared.Map;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinition = default!;

    public void InitializeBuffSystem()
    {
        SubscribeLocalEvent<CultBuffComponent, ComponentAdd>(OnAdd);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateBuffTimers(frameTime);
        AnyCultistNearTile();
        RemoveExpiredBuffs();
    }

    private void AnyCultistNearTile()
    {
        var cultists = EntityQuery<CultistComponent>();

        foreach (var cultist in cultists)
        {
            var uid = cultist.Owner;

            if (HasComp<CultBuffComponent>(uid))
                continue;

            if (!AnyCultTilesNearby(uid))
                continue;

            var comp = EnsureComp<CultBuffComponent>(uid);
            comp.BuffTime = CultBuffComponent.CultTileBuffTime;
        }
    }

    private void OnAdd(EntityUid uid, CultBuffComponent comp, ComponentAdd args)
    {
        _alertsSystem.ShowAlert(uid, AlertType.CultBuffed);
    }

    private void UpdateBuffTimers(float frameTime)
    {
        var buffs = EntityQuery<CultBuffComponent>();

        foreach (var buff in buffs)
        {
            var uid = buff.Owner;
            var remainingTime = buff.BuffTime;

            remainingTime -= TimeSpan.FromSeconds(frameTime);

            if (TryComp<CultistComponent>(uid, out var cultist))
            {
                if (remainingTime < CultBuffComponent.CultTileBuffTime && AnyCultTilesNearby(uid))
                    remainingTime = CultBuffComponent.CultTileBuffTime;
            }

            buff.BuffTime = remainingTime;
        }
    }


    private bool AnyCultTilesNearby(EntityUid uid)
    {
        var localpos = Transform(uid).Coordinates.Position;

        if (!TryComp<CultistComponent>(uid, out var cultist))
            return false;

        var radius = CultBuffComponent.NearbyTilesBuffRadius;

        if (!_mapManager.TryGetGrid(Transform(uid).GridUid, out var grid))
            return false;

        var tilesRefs = grid.GetLocalTilesIntersecting(new Box2(localpos + (-radius, -radius), localpos + (radius, radius)));
        var cultTileDef = (ContentTileDefinition) _tileDefinition[$"{CultRuleComponent.CultFloor}"];
        var cultTile = new Tile(cultTileDef.TileId);

        return tilesRefs.Any(tileRef => tileRef.Tile.TypeId == cultTile.TypeId);
    }

    private void RemoveExpiredBuffs()
    {
        var buffs = EntityQuery<CultBuffComponent>();

        foreach (var buff in buffs)
        {
            var uid = buff.Owner;
            var remainingTime = buff.BuffTime;

            if (remainingTime <= TimeSpan.Zero)
            {
                RemComp<CultBuffComponent>(uid);
                _alertsSystem.ClearAlert(uid, AlertType.CultBuffed);
            }
        }
    }
}
