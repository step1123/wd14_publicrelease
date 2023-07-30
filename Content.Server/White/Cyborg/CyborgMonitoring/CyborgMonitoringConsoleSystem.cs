using System.Linq;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.PowerCell;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.CyborgMonitoring;
using Content.Shared.White.Cyborg.CyborgSensor;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cyborg.CyborgMonitoring;

public sealed class CyborgMonitoringConsoleSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CyborgMonitoringConsoleComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CyborgMonitoringConsoleComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<CyborgMonitoringConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<CyborgMonitoringConsoleComponent, CyborgActionMessage>(OnAction);
    }

    private void OnAction(EntityUid uid, CyborgMonitoringConsoleComponent component, CyborgActionMessage args)
    {
        var status = new CyborgActionStatus(args.Action, args.Address, args.Session.AttachedEntity);
        BroadcastAction(uid, status);
    }

    private void OnRemove(EntityUid uid, CyborgMonitoringConsoleComponent component, ComponentRemove args)
    {
        component.ConnectedSensors.Clear();
    }

    private void OnPacketReceived(EntityUid uid, CyborgMonitoringConsoleComponent component,
        DeviceNetworkPacketEvent args)
    {
        var payload = args.Data;
        // check command
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;
        if (command != DeviceNetworkConstants.CmdUpdatedState)
            return;

        if (!payload.TryGetValue(CyborgSensorConstants.NET_STATUS_COLLECTION,
                out Dictionary<string, CyborgSensorStatus>? sensorStatus))
            return;
        component.ConnectedSensors = sensorStatus;
        UpdateUserInterface(uid, component);
    }

    private void OnUIOpened(EntityUid uid, CyborgMonitoringConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (!_cell.TryUseActivatableCharge(uid))
            return;

        UpdateUserInterface(uid, component);
    }

    private void UpdateUserInterface(EntityUid uid, CyborgMonitoringConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_ui.HasUi(uid, CyborgMonitoringConsoleUiKey.Key))
            return;

        var allSensors = component.ConnectedSensors.Values.ToList();
        var state = new CyborgMonitoringState(allSensors);

        if (_ui.TryGetUi(uid, CyborgMonitoringConsoleUiKey.Key, out var bui))
        {
            UserInterfaceSystem.SetUiState(bui, state);
        }
    }

    private void UpdateAvailableActions(EntityUid uid, List<ActionData> availableAction,
        CyborgMonitoringConsoleComponent? component = null)
    {
    }

    private void BroadcastAction(EntityUid consoleUid, CyborgActionStatus status,
        CyborgMonitoringConsoleComponent? consoleComponent = null, DeviceNetworkComponent? device = null)
    {
        if (!Resolve(consoleUid, ref consoleComponent, ref device))
            return;
        var payload = CyborgActionToPacket(status);
        _deviceNetworkSystem.QueuePacket(consoleUid, status.Address, payload, device: device);
    }

    public NetworkPayload CyborgActionToPacket(CyborgActionStatus status)
    {
        var payload = new NetworkPayload
        {
            [DeviceNetworkConstants.Command] = DeviceNetworkConstants.CmdSetState,
            [CyborgActionConstants.NET_ADDRESS] = status.Address,
            [CyborgActionConstants.NET_ACTION] = status.Action
        };
        if (status.Actioner != null)
            payload.Add(CyborgActionConstants.NET_ACTIONER, status.Actioner);
        return payload;
    }

    public CyborgActionStatus? PacketToCyborgAction(NetworkPayload payload)
    {
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return null;
        if (command != DeviceNetworkConstants.CmdSetState)
            return null;

        if (!payload.TryGetValue(CyborgActionConstants.NET_ADDRESS, out string? address))
            return null;
        if (!payload.TryGetValue(CyborgActionConstants.NET_ACTION, out Enum? action))
            return null;

        payload.TryGetValue(CyborgActionConstants.NET_ACTIONER, out EntityUid actioner);

        var status = new CyborgActionStatus(action, address, actioner);
        return status;
    }
}
