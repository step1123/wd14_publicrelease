using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.IgnitionSource;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Events;

namespace Content.Server.White.Flamethrower;

public sealed class GasProjectileSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasProjectileComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, GasProjectileComponent component, ref StartCollideEvent args)
    {
        if (component.GasMixture is null)
            return;

        var tileInfo = HasComp<AirtightComponent>(args.OtherEntity) ? component.LastTile : component.CurTile;

        if (tileInfo is null)
            return;

        var tile = tileInfo.Value;

        var environment = _atmos.GetTileMixture(tile.GridUid, tile.MapUid, tile.Tile, true);

        if (environment is null)
            return;

        _atmos.Merge(environment, component.GasMixture);

        if (tile.GridUid is null || !TryComp(uid, out IgnitionSourceComponent? ignition) || !ignition.Ignited)
            return;

        _atmos.HotspotExpose(tile.GridUid.Value, tile.Tile, ignition.Temperature, 50, uid, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityManager.EntityQueryEnumerator<GasProjectileComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var proj, out var trans))
        {
            if (proj.GasMixture is null || proj.GasMixture.TotalMoles == 0f)
            {
                QueueDel(uid);
                continue;
            }

            var tileCoords = _transformSystem.GetGridOrMapTilePosition(uid, trans);

            var tileInfo = new TileInfo(trans.GridUid, trans.MapUid, tileCoords);

            if (proj.CurTile == tileInfo)
                continue;

            proj.LastTile = proj.CurTile;
            proj.CurTile = tileInfo;

            var environment = _atmos.GetTileMixture(trans.GridUid, trans.MapUid, tileCoords, true);
            var removed = proj.GasMixture.Remove(proj.GasUsagePerTile);

            if (environment is null)
                continue;

            _atmos.Merge(environment, removed);
        }
    }
}
