﻿using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.UI;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.White.Cult.TimedProduction;

public sealed class CultistFactorySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultistFactoryComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<CultistFactoryComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CultistFactoryComponent, CultistFactoryItemSelectedMessage>(OnSelected);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var structures = EntityQuery<CultistFactoryComponent>();
        foreach (var structure in structures)
        {
            if (structure.Active)
                continue;

            if (_gameTiming.CurTime > structure.NextTimeUse)
            {
                structure.Active = true;
                UpdateAppearance(structure.Owner, structure);
            }
        }
    }

    private void OnInit(EntityUid uid, CultistFactoryComponent component, ComponentInit args)
    {
        UpdateAppearance(uid, component);
    }

    private void OnInteract(EntityUid uid, CultistFactoryComponent component, InteractHandEvent args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        if (!HasComp<CultistComponent>(args.User))
            return;

        if (!CanCraft(uid, component, args.User))
            return;

        if(_ui.TryGetUi(uid, CultistAltarUiKey.Key, out var bui))
        {
            UserInterfaceSystem.SetUiState(bui, new CultistFactoryBUIState(component.Products));
            _ui.OpenUi(bui, actor.PlayerSession);
        }
    }

    private void OnSelected(EntityUid uid, CultistFactoryComponent component, CultistFactoryItemSelectedMessage args)
    {
        var user = args.Session.AttachedEntity;
        if (user == null)
            return;

        if (!CanCraft(uid, component, user.Value))
            return;

        if (!_prototypeManager.TryIndex<CultistFactoryProductionPrototype>(args.Item, out var prototype))
            return;

        foreach (var item in prototype.Item)
        {
            var entity = Spawn(item, Transform(user.Value).Coordinates);
            _handsSystem.TryPickupAnyHand(user.Value, entity);
        }

        component.NextTimeUse = _gameTiming.CurTime + TimeSpan.FromSeconds(component.Cooldown);
        component.Active = false;
        UpdateAppearance(uid, component);
    }

    private bool CanCraft(EntityUid uid, CultistFactoryComponent component, EntityUid user)
    {
        if (component.NextTimeUse == null || _gameTiming.CurTime > component.NextTimeUse)
        {
            component.Active = true;
            UpdateAppearance(uid, component);
            return true;
        }

        var name = MetaData(uid).EntityName;
        var totalSeconds = (component.NextTimeUse - _gameTiming.CurTime).Value.TotalSeconds;
        var seconds = Convert.ToInt32(totalSeconds);

        _popupSystem.PopupEntity(Loc.GetString("cultist-factory-charging", ("name", name),
            ("seconds", seconds)), uid, user);
        UpdateAppearance(uid, component);
        return false;
    }

    private void UpdateAppearance(EntityUid uid, CultistFactoryComponent component)
    {
        AppearanceComponent? appearance = null;
        if (!Resolve(uid, ref appearance))
            return;

        _appearance.SetData(uid, CultCraftStructureVisuals.Activated, component.Active);
    }
}
