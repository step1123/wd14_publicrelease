using Content.Server.Atmos.EntitySystems;
using Content.Server.Mind.Components;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Server.Station.Components;
using Content.Server.White.Cult.Items.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Physics;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Items;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.White.Cult.Items.Systems;

public sealed class TorchCultistsProviderSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TorchCultistsProviderComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<TorchCultistsProviderComponent, TorchWindowItemSelectedMessage>(OnCultistSelected);

        SubscribeLocalEvent<TorchCultistsProviderComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, TorchCultistsProviderComponent component, ComponentInit args)
    {
        UpdateAppearance(uid, component);
    }

    private void OnInteract(EntityUid uid, TorchCultistsProviderComponent comp, AfterInteractEvent args)
    {
        if (!args.Target.HasValue)
        {
            return;
        }

        if (!_interactionSystem.InRangeUnobstructed(args.User, args.Target.Value))
        {
            return;
        }

        if (!TryComp<TorchCultistsProviderComponent>(uid, out var provider))
            return;

        if (!HasComp<CultistComponent>(args.User))
        {
            _hands.TryDrop(args.User);
            _popup.PopupEntity(Loc.GetString("cult-torch-not-cultist"), args.User, args.User);
            return;
        }

        if (!provider.Active || provider.UsesLeft <= 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-torch-drained"), args.User, args.User);
            return;
        }

        if (provider.NextUse > _timing.CurTime)
        {
            _popup.PopupEntity(Loc.GetString("cult-torch-cooldown"), args.User, args.User);
            return;
        }

        if (HasComp<MindContainerComponent>(args.Target))
        {
            TeleportToRandomLocation(uid, args, comp);
            return;
        }

        if (!HasComp<ItemComponent>(args.Target))
        {
            return;
        }

        if (provider.UserInterface == null)
            return;

        provider.ItemSelected = args.Target;

        var cultists = EntityQuery<CultistComponent>();
        var list = new Dictionary<string, string>();

        foreach (var cultist in cultists)
        {
            if (!TryComp<MetaDataComponent>(cultist.Owner, out var meta))
                return;

            if (cultist.Owner == args.User)
                continue;

            list.Add(meta.Owner.ToString(), meta.EntityName);

        }

        if (list.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-torch-cultists-not-found"), args.User, args.User);
            return;
        }

        UserInterfaceSystem.SetUiState(provider.UserInterface, new TorchWindowBUIState(list));

        if (!TryComp<ActorComponent>(args.User, out var actorComponent))
            return;

        _ui.ToggleUi(provider.UserInterface, actorComponent.PlayerSession);
    }

    private void OnCultistSelected(EntityUid uid, TorchCultistsProviderComponent component, TorchWindowItemSelectedMessage args)
    {
        var entityUid = args.Session.AttachedEntity;
        var cultists = EntityQuery<CultistComponent>();

        foreach (var cultist in cultists)
        {
            if (cultist.Owner.ToString() == args.EntUid)
                entityUid = cultist.Owner;
        }

        if (entityUid == args.Session.AttachedEntity && entityUid != null)
        {
            _popup.PopupEntity(Loc.GetString("cult-torch-no-cultist"), entityUid.Value, entityUid.Value);
            return;
        }

        if (component.ItemSelected != null)
        {
            var item = component.ItemSelected.Value;

            if (!TryComp<TransformComponent>(entityUid, out var xForm))
                return;

            _xform.SetCoordinates(item, xForm.Coordinates);
            _hands.PickupOrDrop(entityUid, item);
        }

        UpdateUsesCount(uid, args.Session.AttachedEntity, component);
    }

    private void UpdateAppearance(EntityUid uid, TorchCultistsProviderComponent component)
    {
        AppearanceComponent? appearance = null;
        if (!Resolve(uid, ref appearance, false))
            return;

        _appearance.SetData(uid, VoidTorchVisuals.Activated, component.Active, appearance);
    }

    private void TeleportToRandomLocation(EntityUid torch, AfterInteractEvent args, TorchCultistsProviderComponent component)
    {
        var ownerTransform = Transform(args.User);

        if (_station.GetStationInMap(ownerTransform.MapID) is not { } station ||
            !TryComp<StationDataComponent>(station, out var data) ||
            _station.GetLargestGrid(data) is not { } grid)
        {
            if (ownerTransform.GridUid == null)
                return;
            grid = ownerTransform.GridUid.Value;
        }

        if (!TryComp<MapGridComponent>(grid, out var gridComp))
        {
            return;
        }

        var gridTransform = Transform(grid);
        var gridBounds = gridComp.LocalAABB.Scale(0.7f); // чтобы не заспавнить на самом краю станции

        var targetCoords = gridTransform.Coordinates;

        for (var i = 0; i < 25; i++)
        {
            var randomX = _random.Next((int)gridBounds.Left, (int)gridBounds.Right);
            var randomY = _random.Next((int)gridBounds.Bottom, (int)gridBounds.Top);

            var tile = new Vector2i(randomX, randomY);

            // no air-blocked areas.
            if (_atmosphere.IsTileSpace(grid, gridTransform.MapUid, tile, mapGridComp: gridComp) ||
                _atmosphere.IsTileAirBlocked(grid, tile, mapGridComp: gridComp))
            {
                continue;
            }

            // don't spawn inside of solid objects
            var physQuery = GetEntityQuery<PhysicsComponent>();
            var valid = true;
            foreach (var ent in gridComp.GetAnchoredEntities(tile))
            {
                if (!physQuery.TryGetComponent(ent, out var body))
                    continue;
                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int)CollisionGroup.LargeMobMask) == 0)
                    continue;

                valid = false;
                break;
            }
            if (!valid)
                continue;

            targetCoords = gridComp.GridTileToLocal(tile);
            break;
        }

        _xform.SetCoordinates(args.User, targetCoords);
        _xform.SetCoordinates(args.Target!.Value, targetCoords);

        UpdateUsesCount(torch, args.User, component);
    }

    private void UpdateUsesCount(EntityUid torch, EntityUid? user, TorchCultistsProviderComponent component)
    {
        component.ItemSelected = null;
        component.NextUse = _timing.CurTime + component.Cooldown;
        component.UsesLeft--;

        if (user.HasValue)
        {
            _popup.PopupEntity(Loc.GetString("cult-torch-item-send"), user.Value);
        }

        if (component.UsesLeft <= 0)
        {
            component.Active = false;
            UpdateAppearance(torch, component);

            if (!TryComp<PointLightComponent>(torch, out var light))
                return;

            _pointLight.SetEnabled(torch, false, light);
        }
    }
}
