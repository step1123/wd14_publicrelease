using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Body.Components;
using Content.Server.Chat.Managers;
using Content.Server.Flash;
using Content.Server.Mind.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.White.Cyborg.Laws;
using Content.Server.White.TTS;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Movement.Events;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Content.Shared.White.Cyborg.Systems;
using Content.Shared.White.TTS;
using Content.Shared.Wires;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgSystem : SharedCyborgSystem
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(1);
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly LawsSystem _laws = default!;
    [Dependency] private readonly AccessReaderSystem _reader = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private ISawmill _logger = default!;

    public override void Initialize()
    {
        _logger = Logger.GetSawmill("Borg");
        base.Initialize();

        SubscribeLocalEvent<CyborgComponent, BatteryInsertedEvent>(OnInsertBattery);
        SubscribeLocalEvent<CyborgComponent, RemoveBatteryEvent>(OnRemoveBattery);
        SubscribeLocalEvent<CyborgComponent, ChargeChangedEvent>(OnChargeChange);

        SubscribeLocalEvent<CyborgComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<CyborgComponent, MindRemovedMessage>(OnMindRemoved);

        SubscribeLocalEvent<CyborgComponent, WirePanelDoAfterEvent>(OnUpdateAlert);
        SubscribeLocalEvent<CyborgComponent, PanelLockedEvent>(OnUpdateAlert);
        SubscribeLocalEvent<CyborgComponent, ComponentInit>(OnUpdateAlert);

        SubscribeLocalEvent<CyborgComponent, CyborgGotEmaggedEvent>(OnGotEmagged);
        SubscribeLocalEvent<CyborgComponent, FlashAttemptEvent>(OnFlashAttempt);
        SubscribeLocalEvent<CyborgComponent, UpdateCanMoveEvent>(OnBorgCanMove);
        SubscribeLocalEvent<CyborgComponent, BeingGibbedEvent>(OnGetGibbed);

        SubscribeLocalEvent<CyborgComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
    }

    private void OnGetGibbed(EntityUid uid, CyborgComponent component, BeingGibbedEvent args)
    {
        foreach (var part in component.ModuleUids)
        {
            var ev = new CyborgPartGetGibbedEvent(uid);
            RaiseLocalEvent(part, ev);

            var ee = new ModuleRemoveEvent(part, uid);
            RaiseLocalEvent(part, ee);
        }

        foreach (var part in component.InstrumentUids)
        {
            _logger.Debug($"instrument {part} was gibbed, but was not picked up by the module! Remove then");
            QueueDel(part);
        }

        foreach (var part in args.GibbedParts)
        {
            var ev = new CyborgPartGetGibbedEvent(uid);
            RaiseLocalEvent(part, ev);
        }
    }

    private void OnBorgCanMove(EntityUid uid, CyborgComponent component, UpdateCanMoveEvent args)
    {
        if (component.Energy <= FixedPoint2.Zero || component.Freeze)
            args.Cancel();
    }


    private void OnFlashAttempt(EntityUid uid, CyborgComponent component, FlashAttemptEvent args)
    {
        if (args.User == uid)
        {
            args.Cancel();
            return;
        }

        if (TryComp<CyborgGotFlashedComponent>(uid, out _))
        {
            RemComp<CyborgGotFlashedComponent>(uid);
            return;
        }

        EnsureComp<CyborgGotFlashedComponent>(uid);
        _flash.Flash(uid, args.User, args.Used, 8000, 0.1f);
        args.Cancel();
    }

    private void OnGotEmagged(EntityUid uid, CyborgComponent component, CyborgGotEmaggedEvent args)
    {
        _adminLog.Add(LogType.Emag, LogImpact.Medium,
            $"{ToPrettyString(uid):player} has emmaged by {ToPrettyString(args.User):player}");

        _laws.AddLaw(uid, Loc.GetString("borg-emagged-law", ("person", Identity.Entity(args.User, EntityManager))), 0);
        if (TryComp<ActorComponent>(uid, out var actor))
            _chatManager.DispatchServerMessage(actor.PlayerSession,
                Loc.GetString("borg-emagged-message", ("person", Identity.Entity(args.User, EntityManager))));

        //Raise ModuleGotEmmagedEvent for every module on cyborg
        foreach (var modules in component.InstrumentContainer.ContainedEntities)
        {
            var ev = new ModuleGotEmaggedEvent(args.User, uid);
            RaiseLocalEvent(modules, ev);
        }
    }


    private void OnUpdateAlert(EntityUid uid, CyborgComponent component, EntityEventArgs args)
    {
        UpdateAlert(uid, component);
        Dirty(component);
        AddTtsComponent(uid);
    }

    private void AddTtsComponent(EntityUid uid)
    {
        var ttsComp = EnsureComp<TTSComponent>(uid);
        var borgVoices = _prototypeManager.EnumeratePrototypes<TTSVoicePrototype>()
            .Where(voice => voice.BorgVoice).ToList();

        var voice = borgVoices.ElementAt(new Random().Next(borgVoices.Count));
        ttsComp.VoicePrototypeId = voice.ID;
    }

    private void OnGetAdditionalAccess(EntityUid uid, CyborgComponent component, ref GetAdditionalAccessEvent args)
    {
        args.Entities.Add(uid);
        args.Entities.UnionWith(component.ModuleContainer.ContainedEntities);
    }

    private void OnChargeChange(EntityUid uid, CyborgComponent component, ref ChargeChangedEvent args)
    {
        UpdateAlert(uid, component);
        if (args.Charge == 0 && component.BatterySlot.ContainedEntity != null)
        {
            _adminLog.Add(LogType.Emag, LogImpact.High,
                $"{ToPrettyString(uid):player} borg have not enought energy!");

            var ev = new BatteryLowEvent(uid, component.BatterySlot.ContainedEntity.Value);
            RaiseLocalEvent(uid, ev);
        }

        _actionBlocker.UpdateCanMove(uid);
        UpdateUserInterface(uid, component);
    }

    private void OnInsertBattery(EntityUid uid, CyborgComponent component, BatteryInsertedEvent args)
    {
        if (!TryComp<BatteryComponent>(args.Battery, out var battery))
            return;
        InsertBattery(uid, args.Battery, component, battery);
    }

    private void OnRemoveBattery(EntityUid uid, CyborgComponent component, RemoveBatteryEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        RemoveBattery(uid, component);
        args.Handled = true;
    }

    private void OnMindRemoved(EntityUid uid, CyborgComponent component, MindRemovedMessage args)
    {
        if (_ui.HasUi(uid, CyborgInstrumentSelectUiKey.Key))
        {
            var ui = _ui.GetUi(uid, CyborgInstrumentSelectUiKey.Key);
            _ui.CloseAll(ui);
        }

        component.Active = false;
        Dirty(component);
    }

    private void OnMindAdded(EntityUid uid, CyborgComponent component, MindAddedMessage args)
    {
        _adminLog.Add(LogType.Emag, LogImpact.Low,
            $"{ToPrettyString(uid):player} borg has initialized!");
        component.Active = true;
        Dirty(component);
    }


    /// <summary>
    ///     Checks whether this entity has access to the borg
    /// </summary>
    /// <param name="cyborgUid"></param>
    /// <param name="uid">Entity`s Uid</param>
    /// <seealso cref="SharedCyborgSystem.HasAccess" />
    /// <returns>Is has access to the borg</returns>
    public bool HasAccess(EntityUid cyborgUid, EntityUid? uid)
    {
        if (!uid.HasValue)
            return false;

        if (_adminManager.IsAdmin(uid.Value))
            return true;

        // Checks if another entity is a Borg. If yes then return false
        if (HasComp<CyborgComponent>(uid))
            return false;

        if (TryComp<CyborgEmaggedComponent>(uid, out var emaggedComponent))
            return uid == emaggedComponent.EmmagedBy;

        var accessTags = _reader.FindAccessTags(uid.Value).ToHashSet();
        return HasAccess(cyborgUid, accessTags);
    }


    /// <summary>
    ///     Responsible for displaying and updating Borg's Alerts
    /// </summary>
    /// <param name="uid">Cyborg`s Uid</param>
    /// <param name="component"></param>
    public void UpdateAlert(EntityUid uid, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        //Battery status alert
        var per = 0;
        if (component.MaxEnergy > 0)
            per = (int) (component.Energy / component.MaxEnergy * 6);
        _alerts.ShowAlert(uid, AlertType.Charge, (short) per);
        if (!TryComp<WiresPanelComponent>(uid, out var wiresPanelComponent))
            return;

        //Panel is open alert
        if (wiresPanelComponent.Open)
            _alerts.ShowAlert(uid, AlertType.Panel, (short) (component.PanelLocked ? 0 : 1));
        else if (_alerts.IsShowingAlert(uid, AlertType.Panel))
            _alerts.ClearAlert(uid, AlertType.Panel);
    }

    public override void UpdateUserInterface(EntityUid uid, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        base.UpdateUserInterface(uid, component);
        if (!_ui.HasUi(uid, CyborgInstrumentSelectUiKey.Key))
            return;

        var state = new CyborgInstrumentSelectListState(component.InstrumentUids);
        var ui = _ui.GetUi(uid, CyborgInstrumentSelectUiKey.Key);

        UserInterfaceSystem.SetUiState(ui, state);
    }


    public void InsertBattery(EntityUid uid, EntityUid toInsert, CyborgComponent? component = null,
        BatteryComponent? battery = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (!Resolve(toInsert, ref battery, false))
            return;

        component.BatterySlot.Insert(toInsert);
        component.Energy = battery.CurrentCharge;
        component.MaxEnergy = battery.MaxCharge;

        var ev = new ChargeChangedEvent
        {
            Charge = component.Energy.Float(),
            MaxCharge = component.MaxEnergy.Float()
        };
        RaiseLocalEvent(uid, ref ev);

        Dirty(component);
    }

    public void RemoveBattery(EntityUid uid, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;


        _container.EmptyContainer(component.BatterySlot);
        component.Energy = 0;
        component.MaxEnergy = 0;

        var ev = new ChargeChangedEvent
        {
            Charge = component.Energy.Float(),
            MaxCharge = component.MaxEnergy.Float()
        };
        RaiseLocalEvent(uid, ref ev);

        Dirty(component);
    }


    public override bool TryChangeEnergy(EntityUid uid, FixedPoint2 delta, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (!base.TryChangeEnergy(uid, delta, component))
            return false;

        var battery = component.BatterySlot.ContainedEntity;
        if (battery == null)
            return false;

        if (!TryComp<BatteryComponent>(battery, out var batteryComp))
            return false;

        _battery.SetCharge(battery.Value, batteryComp.CurrentCharge + delta.Float(), batteryComp);
        if (batteryComp.CurrentCharge != component.Energy) //if there's a discrepency, we have to resync them
        {
            component.Energy = batteryComp.CurrentCharge;
            Dirty(component);
        }

        var ev = new ChargeChangedEvent
        {
            Charge = component.Energy.Float(),
            MaxCharge = component.MaxEnergy.Float()
        };
        Dirty(component);
        RaiseLocalEvent(uid, ref ev);
        return true;
    }

    public void TransferEnergy(EntityUid uid, EntityUid transferEntity, float value,
        CyborgComponent? component = null, BatteryComponent? batteryComponent = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(transferEntity, ref batteryComponent))
            return;
        if (batteryComponent.Charge >= batteryComponent.MaxCharge)
            return;

        var changedCharge = float.Min(batteryComponent.Charge + value, batteryComponent.MaxCharge);

        if (changedCharge < 0 || !TryChangeEnergy(uid, -(changedCharge - batteryComponent.Charge), component))
            return;
        _battery.SetCharge(transferEntity, changedCharge, batteryComponent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CyborgComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.NextUpdateTime)
                continue;
            component.NextUpdateTime += Delay;

            if (!component.Active || component.Consumption == 0 || component.Energy == 0)
                return;

            component.Energy = FixedPoint2.Max(FixedPoint2.Zero, component.Energy);

            TryChangeEnergy(uid, -component.Consumption, component);
        }
    }
}
