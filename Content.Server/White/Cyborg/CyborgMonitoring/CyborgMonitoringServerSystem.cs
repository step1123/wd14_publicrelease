﻿using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Server.White.Cyborg.CyborgSensor;
using Content.Shared.White.Cyborg.CyborgMonitoring;
using Content.Shared.White.Cyborg.CyborgSensor;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.CyborgMonitoring;

public sealed class CyborgMonitoringServerSystem : EntitySystem
{
    private const float UpdateRate = 3f;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly CyborgSensorSystem _sensors = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    private float _updateDiff;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CyborgMonitoringServerComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CyborgMonitoringServerComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<CyborgMonitoringServerComponent, PowerChangedEvent>(OnPowerChanged);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // check update rate
        _updateDiff += frameTime;
        if (_updateDiff < UpdateRate)
            return;
        _updateDiff -= UpdateRate;

        var query = EntityQueryEnumerator<CyborgMonitoringServerComponent>();
        List<EntityUid> activeServers = new();

        while (query.MoveNext(out var uid, out var server))
        {
            //Make sure the server is disconnected when it becomes unavailable
            if (!server.Available)
            {
                if (server.Active)
                    DisconnectServer(uid, server);

                continue;
            }

            if (!server.Active)
                continue;

            activeServers.Add(uid);
        }


        foreach (var activeServer in activeServers)
        {
            UpdateTimeout(activeServer);
            BroadcastSensorStatus(activeServer);
        }
    }

    public bool TryGetActiveServerAddress(EntityUid stationId, out string? address)
    {
        (EntityUid, CyborgMonitoringServerComponent, DeviceNetworkComponent)? last = default;

        var query = EntityQueryEnumerator<CyborgMonitoringServerComponent, DeviceNetworkComponent>();
        while (query.MoveNext(out var uid, out var server, out var device))
        {
            if (!_stationSystem.GetOwningStation(uid)?.Equals(stationId) ?? false)
                continue;

            if (!server.Available)
            {
                DisconnectServer(uid, server, device);
                continue;
            }

            last = (uid, server, device);

            if (server.Active)
            {
                address = device.Address;
                return true;
            }
        }


        //If there was no active server for the station make the last available inactive one active
        if (last.HasValue)
        {
            ConnectServer(last.Value.Item1, last.Value.Item2, last.Value.Item3);
            address = last.Value.Item3.Address;
            return true;
        }

        address = null;
        return address != null;
    }

    /// <summary>
    ///     Adds or updates a sensor status entry if the received package is a sensor status update
    /// </summary>
    private void OnPacketReceived(EntityUid uid, CyborgMonitoringServerComponent component,
        DeviceNetworkPacketEvent args)
    {
        var sensorStatus = _sensors.PacketToCyborgSensor(args.Data);
        if (sensorStatus == null)
            return;

        sensorStatus.Timestamp = _gameTiming.CurTime;
        component.SensorStatus[args.SenderAddress] = sensorStatus;
    }

    /// <summary>
    ///     Clears the servers sensor status list
    /// </summary>
    private void OnRemove(EntityUid uid, CyborgMonitoringServerComponent component, ComponentRemove args)
    {
        component.SensorStatus.Clear();
    }

    /// <summary>
    ///     Disconnects the server losing power
    /// </summary>
    private void OnPowerChanged(EntityUid uid, CyborgMonitoringServerComponent component, ref PowerChangedEvent args)
    {
        component.Available = args.Powered;

        if (!args.Powered)
            DisconnectServer(uid, component);
    }

    /// <summary>
    ///     Drop the sensor status if it hasn't been updated for to long
    /// </summary>
    private void UpdateTimeout(EntityUid uid, CyborgMonitoringServerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        foreach (var (address, sensor) in component.SensorStatus)
        {
            var dif = _gameTiming.CurTime - sensor.Timestamp;
            if (dif.Seconds > component.SensorTimeout)
                component.SensorStatus.Remove(address);
        }
    }

    /// <summary>
    ///     Broadcasts the status of all connected sensors
    /// </summary>
    private void BroadcastSensorStatus(EntityUid uid, CyborgMonitoringServerComponent? serverComponent = null,
        DeviceNetworkComponent? device = null)
    {
        if (!Resolve(uid, ref serverComponent, ref device))
            return;
        var payload = new NetworkPayload
        {
            [DeviceNetworkConstants.Command] = DeviceNetworkConstants.CmdUpdatedState,
            [CyborgSensorConstants.NET_STATUS_COLLECTION] = serverComponent.SensorStatus
        };

        _deviceNetworkSystem.QueuePacket(uid, null, payload, device: device);
    }

    private void ConnectServer(EntityUid uid, CyborgMonitoringServerComponent? server = null,
        DeviceNetworkComponent? device = null)
    {
        if (!Resolve(uid, ref server, ref device))
            return;

        server.Active = true;

        if (_deviceNetworkSystem.IsDeviceConnected(uid, device))
            return;

        _deviceNetworkSystem.ConnectDevice(uid, device);
    }

    /// <summary>
    ///     Disconnects a server from the device network and clears the currently active server
    /// </summary>
    private void DisconnectServer(EntityUid uid, CyborgMonitoringServerComponent? server = null,
        DeviceNetworkComponent? device = null)
    {
        if (!Resolve(uid, ref server, ref device))
            return;

        server.SensorStatus.Clear();
        server.Active = false;

        _deviceNetworkSystem.DisconnectDevice(uid, device, false);
    }
}
