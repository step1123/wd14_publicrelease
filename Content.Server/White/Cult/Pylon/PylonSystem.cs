using System.Linq;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Maps;
using Content.Shared.Damage;
using Content.Shared.Doors.Components;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Pylon;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.White.Cult.Pylon;

public sealed class PylonSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinition = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SharedPylonComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<SharedPylonComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, SharedPylonComponent component, ComponentInit args)
    {
        UpdateAppearance(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var pylonsQuery = EntityQuery<SharedPylonComponent>();

        foreach (var comp in pylonsQuery)
        {
            if (comp.NextTileConvert == TimeSpan.Zero)
                comp.NextTileConvert = _timing.CurTime + TimeSpan.FromSeconds(comp.TileConvertCooldown);

            if (comp.NextHealTime == TimeSpan.Zero)
                comp.NextHealTime = _timing.CurTime + TimeSpan.FromSeconds(comp.HealingAuraCooldown);

            if (_timing.CurTime >= comp.NextHealTime)
            {
                comp.NextHealTime = _timing.CurTime + TimeSpan.FromSeconds(comp.HealingAuraCooldown);

                if (comp.Activated)
                    HealPlayersInRange(comp);
            }

            if (_timing.CurTime >= comp.NextTileConvert)
            {
                comp.NextTileConvert = _timing.CurTime + TimeSpan.FromSeconds(comp.TileConvertCooldown);

                if (comp.Activated)
                    ConvertNearbyTiles(comp);
            }
        }
    }

    private void ConvertNearbyTiles(SharedPylonComponent comp)
    {
        var tilesConverted = 0;
        var random = new Random().Next(1, 3);

        var uid = comp.Owner;
        var gridUid = Transform(uid).GridUid;
        var pylonPos = Transform(uid).Coordinates;

        if (!_mapManager.TryGetGrid(gridUid, out var grid))
            return;

        var radius = comp.TileConvertRange;
        var tilesRefs = grid.GetLocalTilesIntersecting(new Box2(pylonPos.Position + (-radius, -radius),
            pylonPos.Position + (radius, radius)));
        var tiles = ShuffleTiles(tilesRefs);

        if (comp.ConvertEverything)
            ConvertEverything(comp, tiles);

        var cultTileDef = (ContentTileDefinition) _tileDefinition[$"{comp.TileId}"];
        var cultTile = new Tile(cultTileDef.TileId);

        foreach (var tile in tiles)
        {
            if (tilesConverted >= random)
                return;

            var tilePos = tile.GridPosition();

            if (pylonPos.InRange(EntityManager, tilePos, comp.TileConvertRange))
            {
                if (tile.Tile.TypeId == cultTile.TypeId)
                    continue;

                _tile.ReplaceTile(tile, cultTileDef);
                _entMan.SpawnEntity(comp.TileConvertEffect, tilePos);
                _audio.PlayPvs(comp.ConvertTileSound, tilePos, AudioParams.Default.WithVolume(-5));

                tilesConverted++;
            }
        }
    }

    private void ConvertEverything(SharedPylonComponent comp, IEnumerable<TileRef> tiles)
    {
        foreach (var tile in tiles)
        {
            if (!_turf.IsTileBlocked(tile, CollisionGroup.WallLayer)
                || !_turf.IsTileBlocked(tile, CollisionGroup.AirlockLayer))
                continue;

            foreach (var entity in _lookup.GetEntitiesIntersecting(tile.GridPosition()))
            {
                if (TryComp<TagComponent>(entity, out var tag)
                    && tag.Tags.Contains("Wall")
                    && MetaData(entity).EntityPrototype?.ID != comp.WallId)
                {
                    _entMan.SpawnEntity(comp.WallId, Transform(entity).Coordinates);
                    _entMan.SpawnEntity(comp.WallConvertEffect, Transform(entity).Coordinates);
                    _entMan.DeleteEntity(entity);
                    _audio.PlayPvs(comp.ConvertTileSound, tile.GridPosition(), AudioParams.Default.WithVolume(-10));
                    return;
                }

                if (HasComp<AirlockComponent>(entity) && MetaData(entity).EntityPrototype?.ID != comp.AirlockId)
                {
                    _entMan.SpawnEntity(comp.AirlockId, Transform(entity).Coordinates);
                    _entMan.SpawnEntity(comp.AirlockConvertEffect, Transform(entity).Coordinates);
                    _entMan.DeleteEntity(entity);
                    _audio.PlayPvs(comp.ConvertTileSound, tile.GridPosition(), AudioParams.Default.WithVolume(-10));
                    return;
                }
            }
        }
    }

    private void HealPlayersInRange(SharedPylonComponent comp)
    {
        foreach (var player in _playerManager.Sessions)
        {
            if (player.AttachedEntity is not { Valid: true } playerEntity)
                continue;

            if (!EntityManager.TryGetComponent<CultistComponent>(playerEntity, out _))
                continue;

            if (_mobStateSystem.IsDead(playerEntity))
                continue;

            var playerDamageComp = EntityManager.TryGetComponent<DamageableComponent>(playerEntity, out var damageComp)
                ? damageComp
                : null;

            if (playerDamageComp == null || playerDamageComp.Damage.Total == 0)
                continue;

            var uid = comp.Owner;
            var pylonXForm = Transform(uid);
            var playerXForm = Transform(playerEntity);

            if (pylonXForm.Coordinates.InRange(EntityManager, playerXForm.Coordinates, comp.HealingAuraRange))
            {
                var damage = comp.HealingAuraDamage;
                _damageSystem.TryChangeDamage(playerEntity, damage, true);

                if (!TryComp<BloodstreamComponent>(playerEntity, out var bloodstream))
                    continue;

                if (bloodstream.IsBleeding)
                {
                    _blood.TryModifyBleedAmount(playerEntity, -comp.BleedReductionAmount, bloodstream);
                }

                if (_blood.GetBloodLevelPercentage(playerEntity, bloodstream) < bloodstream.BloodMaxVolume)
                {
                    _blood.TryModifyBloodLevel(playerEntity, comp.BloodRefreshAmount, bloodstream);
                }
            }
        }
    }

    private void OnInteract(EntityUid uid, SharedPylonComponent comp, InteractHandEvent args)
    {
        var user = args.User;
        var pylon = args.Target;

        if (HasComp<CultistComponent>(user))
        {
            comp.Activated = !comp.Activated;

            UpdateAppearance(uid, comp);

            if (!TryComp<PointLightComponent>(uid, out var light))
                return;

            light.Enabled = comp.Activated;

            var toggleMsg = Loc.GetString(comp.Activated ? "pylon-toggle-on" : "pylon-toggle-off");
            _popupSystem.PopupEntity(toggleMsg, uid);
            return;
        }

        var damage = comp.BurnDamageOnInteract;
        var burnMsg = Loc.GetString("powered-light-component-burn-hand");

        _audio.Play(comp.BurnHandSound, Filter.Pvs(pylon), pylon, true);
        _popupSystem.PopupEntity(burnMsg, pylon, user);
        _damageSystem.TryChangeDamage(user, damage, true);
    }

    private IEnumerable<TileRef> ShuffleTiles(IEnumerable<TileRef> collection)
    {
        var random = new Random();
        var shuffledList = collection.ToList();

        var n = shuffledList.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (shuffledList[k], shuffledList[n]) = (shuffledList[n], shuffledList[k]);
        }

        return shuffledList;
    }

    private void UpdateAppearance(EntityUid uid, SharedPylonComponent comp)
    {
        AppearanceComponent? appearance = null;
        if (!Resolve(uid, ref appearance, false))
            return;

        _appearance.SetData(uid, PylonVisuals.Activated, comp.Activated, appearance);
    }
}
