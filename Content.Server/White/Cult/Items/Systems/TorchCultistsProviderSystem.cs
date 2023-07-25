using Content.Server.Popups;
using Content.Server.White.Cult.Items.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Items;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.White.Cult.Items.Systems;

public sealed class TorchCultistsProviderSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

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

        _ui.SetUiState(provider.UserInterface, new TorchWindowBUIState(list));

        if(!TryComp<ActorComponent>(args.User, out var actorComponent))
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

        component.ItemSelected = null;
        component.NextUse = _timing.CurTime + component.Cooldown;
        component.UsesLeft--;

        if (args.Session.AttachedEntity != null)
            _popup.PopupEntity(Loc.GetString("cult-torch-item-send"), args.Session.AttachedEntity.Value);

        if (component.UsesLeft <= 0)
        {
            component.Active = false;
            UpdateAppearance(uid, component);

            if (!TryComp<PointLightComponent>(uid, out var light))
                return;

            light.Enabled = false;
        }
    }

    private void UpdateAppearance(EntityUid uid, TorchCultistsProviderComponent component)
    {
        AppearanceComponent? appearance = null;
        if (!Resolve(uid, ref appearance, false))
            return;

        _appearance.SetData(uid, VoidTorchVisuals.Activated, component.Active, appearance);
    }
}
