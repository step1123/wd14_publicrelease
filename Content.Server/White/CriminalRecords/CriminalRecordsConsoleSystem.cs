using System.Linq;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.PDA;
using Content.Shared.Radio;
using Content.Shared.StationRecords;
using Content.Shared.White.CriminalRecords;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.White.CriminalRecords;

public sealed class CriminalRecordsConsoleSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecordsSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, BoundUIOpenedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, SelectCriminalRecord>(OnKeySelected);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, SelectCriminalStatus>(OnStatusSelected);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, SelectCriminalReason>(OnReasonSelected);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);
    }

    private void OnComponentInit(EntityUid uid, CriminalRecordsConsoleComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, CriminalRecordsConsoleComponent.IdSlotId, component.IdSlot);
    }

    private void OnComponentRemove(EntityUid uid, CriminalRecordsConsoleComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.IdSlot);
    }

    private void UpdateUserInterface<T>(EntityUid uid, CriminalRecordsConsoleComponent component, T ev)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnItemInserted(EntityUid uid, CriminalRecordsConsoleComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == CriminalRecordsConsoleComponent.IdSlotId)
            component.ContainedID = CompOrNull<IdCardComponent>(args.Entity);

        UpdateUserInterface(uid, component);
    }

    private void OnItemRemoved(EntityUid uid, CriminalRecordsConsoleComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID == component.IdSlot.ID)
            component.ContainedID = null;

        UpdateUserInterface(uid, component);
    }

    private void OnKeySelected(EntityUid uid, CriminalRecordsConsoleComponent component,
        SelectCriminalRecord msg)
    {
        component.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(uid, component);
    }

    private void OnReasonSelected(EntityUid uid, CriminalRecordsConsoleComponent component,
        SelectCriminalReason msg)
    {
        var hasServer = new EventCheckServer();
        RaiseLocalEvent(hasServer);

        if (!hasServer.Result)
        {
            UpdateUserInterface(uid, component);
            return;
        }

        var ev = new EventChangeReason(msg.SelectedKey, msg.Text);
        RaiseLocalEvent(ev);

        UpdateUserInterface(uid, component);
    }

    private void OnStatusSelected(EntityUid uid, CriminalRecordsConsoleComponent component,
        SelectCriminalStatus msg)
    {
        if (msg.SelectedStatus == null)
            return;

        var hasServer = new EventCheckServer();
        RaiseLocalEvent(hasServer);

        if (!hasServer.Result)
        {
            UpdateUserInterface(uid, component);
            return;
        }

        var messageId = "null";
        switch (msg.SelectedStatus.CriminalType)
        {
            case EnumCriminalRecordType.Released:
                messageId = "criminal-targetchannel-set-released";
                break;
            case EnumCriminalRecordType.Discharged:
                messageId = "criminal-targetchannel-set-discharged";
                break;
            case EnumCriminalRecordType.Parolled:
                messageId = "criminal-targetchannel-set-parolled";
                break;
            case EnumCriminalRecordType.Suspected:
                messageId = "criminal-targetchannel-set-suspected";
                break;
            case EnumCriminalRecordType.Wanted:
                messageId = "criminal-targetchannel-set-wanted";
                break;
            case EnumCriminalRecordType.Incarcerated:
                messageId = "criminal-targetchannel-set-incarcerated";
                break;
        }

        var message = "";

        if (msg.SelectedStatus.Reason != string.Empty)
        {
            messageId += "-reason";
            message = Loc.GetString(messageId,
                ("target", msg.SelectedStatus.StationRecord.Name),
                ("reason", msg.SelectedStatus.Reason));
        }
        else
        {
            message = Loc.GetString(messageId,
                ("target", msg.SelectedStatus.StationRecord.Name));
        }

        _radioSystem.SendRadioMessage(uid, message, _prototype.Index<RadioChannelPrototype>(component.TargetChannel), uid);

        var ev = new EventChangeCache(msg.SelectedKey, msg.SelectedStatus);
        RaiseLocalEvent(ev);

        UpdateUserInterface(uid, component);
    }

    private void UpdateUserInterface(EntityUid uid,
        CriminalRecordsConsoleComponent? console = null)
    {
        if (!Resolve(uid, ref console))
        {
            return;
        }

        Dirty(console);

        var owningStation = _stationSystem.GetOwningStation(uid);

        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecordsComponent))
        {
            CriminalRecordsConsoleBuiState state = new(null, null, null, null, null, false, false); //null
            SetStateForInterface(uid, state);
            return;
        }

        var consoleRecords =
            _stationRecordsSystem.GetRecordsOfType<GeneralStationRecord>(owningStation.Value, stationRecordsComponent);

        var listing = new Dictionary<StationRecordKey, string>();

        foreach (var pair in consoleRecords)
        {
            listing.Add(pair.Item1, pair.Item2.Name);
        }

        if (listing.Count == 0)
        {
            CriminalRecordsConsoleBuiState state = new(null, null, null, null, null, false, false); //console!.Filter
            SetStateForInterface(uid, state);
            return;
        }
        else if (listing.Count == 1)
        {
            console!.ActiveKey = listing.Keys.First();
        }

        GeneralStationRecord? record = null;
        if (console!.ActiveKey != null)
        {
            _stationRecordsSystem.TryGetRecord(owningStation.Value, console.ActiveKey.Value, out record,
                stationRecordsComponent);
        }

        var serverEv = new EventGetCache();
        RaiseLocalEvent(serverEv);

        var hasServer = new EventCheckServer();
        RaiseLocalEvent(hasServer);

        var idCardInfo = console.ContainedID != null ? new IdCardNetInfo(console.ContainedID.FullName, console.ContainedID.JobTitle) : null;

        CriminalRecordsConsoleBuiState newState = new(console.ActiveKey, record, listing, serverEv.Cache, idCardInfo, AccessCheck(console.ContainedID), hasServer.Result); //console.Filter
        SetStateForInterface(uid, newState);
    }

    private void SetStateForInterface(EntityUid uid, CriminalRecordsConsoleBuiState newState)
    {
        if (_userInterface.TryGetUi(uid, CriminalRecordsConsoleKey.Key, out var bui))
            UserInterfaceSystem.SetUiState(bui, newState);
    }

    private bool AccessCheck(IdCardComponent? component)
    {
        if (component is null)
            return false;

        var uid = component.Owner;

        if (!EntityManager.TryGetComponent(uid, out AccessComponent? reader))
            return false;

        foreach (var tag in reader.Tags)
        {
            var proto = _prototype.Index<EntityPrototype>("ComputerCriminalRecords");
            proto.TryGetComponent(out AccessReaderComponent? access);

            if (access == null)
                continue;

            if (access.AccessLists.SelectMany(list => list).Any(entry => entry == tag))
            {
                return true;
            }
        }

        return false;
    }
}
