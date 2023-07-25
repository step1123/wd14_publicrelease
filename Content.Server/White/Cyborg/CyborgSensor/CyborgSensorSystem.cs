using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Station.Systems;
using Content.Server.White.Cyborg.CyborgMonitoring;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.CyborgSensor;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.CyborgSensor;

public sealed class CyborgSensorSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly CyborgMonitoringServerSystem _monitoringServerSystem = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CyborgSensorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CyborgSensorComponent, EntityUnpausedEvent>(OnUnpaused);
    }

    private void OnUnpaused(EntityUid uid, CyborgSensorComponent component, ref EntityUnpausedEvent args)
    {
        component.NextUpdate += args.PausedTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _gameTiming.CurTime;
        var sensors = EntityManager.EntityQueryEnumerator<CyborgSensorComponent, DeviceNetworkComponent>();

        while (sensors.MoveNext(out var uid, out var sensor, out var device))
        {
            if (device.TransmitFrequency is null || !sensor.StationId.HasValue)
                continue;

            // check if sensor is ready to update
            if (curTime < sensor.NextUpdate)
                continue;

            sensor.NextUpdate = curTime + sensor.UpdateRate;

            // get sensor status
            var status = GetSensorState(uid, sensor);
            if (status == null)
                continue;

            //Retrieve active server address if the sensor isn't connected to a server
            if (sensor.ConnectedServer == null)
            {
                if (!_monitoringServerSystem.TryGetActiveServerAddress(sensor.StationId.Value, out var address))
                    continue;

                sensor.ConnectedServer = address;
            }

            // Send it to the connected server
            var payload = CyborgSensorToPacket(status);

            // Clear the connected server if its address isn't on the network
            if (!_deviceNetworkSystem.IsAddressPresent(device.DeviceNetId, sensor.ConnectedServer))
            {
                sensor.ConnectedServer = null;
                continue;
            }


            _deviceNetworkSystem.QueuePacket(uid, sensor.ConnectedServer, payload, device: device);
        }
    }



    private void OnMapInit(EntityUid uid, CyborgSensorComponent component, MapInitEvent args)
    {
        component.StationId ??= _stationSystem.GetOwningStation(uid);
    }

    public CyborgSensorStatus? GetSensorState(EntityUid uid, CyborgSensorComponent? sensor = null,
        TransformComponent? transform = null, CyborgComponent? component = null, DeviceNetworkComponent? deviceNetworkComponent = null)
        {
            if (!Resolve(uid, ref sensor, ref transform, ref component, ref deviceNetworkComponent))
                return null;

            // try to get mobs id from metaData
            var userName = Loc.GetString("suit-sensor-component-unknown-name");
            if (TryComp<MetaDataComponent>(uid, out var metaDataComponent))
            {
                userName = metaDataComponent.EntityName;
            }

            // get health mob state
            var isAlive = false;
            if (TryComp<MobStateComponent>(uid,out var mobState))
                isAlive = !_mobStateSystem.IsDead(uid, mobState);


            // finally, form suit sensor status
            var status = new CyborgSensorStatus(userName,deviceNetworkComponent.Address);
            status.IsAlive = isAlive;
            status.Freeze = component.Freeze;
            EntityCoordinates coordinates;
            var xformQuery = GetEntityQuery<TransformComponent>();
            if (transform.GridUid != null)
            {
                coordinates = new EntityCoordinates(transform.GridUid.Value,
                    _transform.GetInvWorldMatrix(xformQuery.GetComponent(transform.GridUid.Value), xformQuery)
                    .Transform(_transform.GetWorldPosition(transform, xformQuery)));
            }
            else if (transform.MapUid != null)
            {
                coordinates = new EntityCoordinates(transform.MapUid.Value,
                    _transform.GetWorldPosition(transform, xformQuery));
            }
            else
            {
                coordinates = EntityCoordinates.Invalid;
            }

            status.Prototype = component.Prototype;
            status.Coordinates = coordinates;
            status.Energy = component.Energy;
            status.MaxEnergy = component.MaxEnergy;
            status.IsActive = component.Active;
            status.IsPanelLocked = component.PanelLocked;
            status.АvailableAction = component.ActionsData;
            return status;
        }


    /// <summary>
    ///     Serialize create a device network package from the suit sensors status.
    /// </summary>
    public NetworkPayload CyborgSensorToPacket(CyborgSensorStatus status)
    {
        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = DeviceNetworkConstants.CmdUpdatedState,
            [CyborgSensorConstants.NET_NAME] = status.Name,
            [CyborgSensorConstants.NET_ADDRESS] = status.Address,
            [CyborgSensorConstants.NET_IS_ALIVE] = status.IsAlive,
            [CyborgSensorConstants.NET_ENERGY] = status.Energy,
            [CyborgSensorConstants.NET_COORDINATES] = status.Coordinates,
            [CyborgSensorConstants.NET_MAX_ENERGY] = status.MaxEnergy,
            [CyborgSensorConstants.NET_IS_ACTIVE] = status.IsActive,
            [CyborgSensorConstants.NET_IS_PANEL_LOCKED] = status.IsPanelLocked,
            [CyborgSensorConstants.NET_FREEZE] = status.Freeze
        };
        if (status.Prototype != null)
        {
            payload.Add(CyborgSensorConstants.NET_PROTOTYPE,status.Prototype);
        }

        if (status.АvailableAction != null)
        {
            payload.Add(CyborgSensorConstants.NET_AVAILABLE_ACTION,status.АvailableAction);
        }

        return payload;
    }

    /// <summary>
    ///     Try to create the suit sensors status from the device network message
    /// </summary>
    public CyborgSensorStatus? PacketToCyborgSensor(NetworkPayload payload)
    {
        // check command
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return null;
        if (command != DeviceNetworkConstants.CmdUpdatedState)
            return null;

        // check some shit
        if (!payload.TryGetValue(CyborgSensorConstants.NET_NAME, out string? name)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_ADDRESS, out string? address)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_IS_ALIVE, out bool isAlive)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_IS_ACTIVE, out bool isActive)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_IS_PANEL_LOCKED, out bool isPanelLocked)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_COORDINATES, out EntityCoordinates cords)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_ENERGY, out FixedPoint2 energy)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_MAX_ENERGY, out FixedPoint2 maxEnergy)) return null;
        if (!payload.TryGetValue(CyborgSensorConstants.NET_FREEZE, out bool freeze)) return null;

        payload.TryGetValue(CyborgSensorConstants.NET_AVAILABLE_ACTION,
            out List<ActionData>? availableAction);
        payload.TryGetValue(CyborgSensorConstants.NET_PROTOTYPE, out string? prototype);

        var status = new CyborgSensorStatus(name, address)
        {
            Prototype = prototype,
            IsAlive = isAlive,
            IsActive = isActive,
            IsPanelLocked = isPanelLocked,
            Coordinates = cords,
            Energy = energy,
            MaxEnergy = maxEnergy,
            АvailableAction = availableAction,
            Freeze = freeze
        };

        return status;
    }

}
