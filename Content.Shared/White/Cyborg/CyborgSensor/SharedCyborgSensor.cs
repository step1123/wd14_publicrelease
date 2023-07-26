using Content.Shared.FixedPoint;
using Content.Shared.White.Cyborg.Components;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.CyborgSensor;

[Serializable]
[NetSerializable]
public sealed class CyborgSensorStatus
{
    public List<ActionData>? АvailableAction;

    public string Address;
    public EntityCoordinates Coordinates;
    public FixedPoint2 Energy;
    public bool Freeze;
    public bool IsActive;
    public bool IsAlive;
    public bool IsPanelLocked;
    public FixedPoint2 MaxEnergy;
    public string Name;
    public string? Prototype;
    public TimeSpan Timestamp;

    public CyborgSensorStatus(string name, string address)
    {
        Name = name;
        Address = address;
    }
}

public static class CyborgSensorConstants
{
    public const string NET_ADDRESS = "address";
    public const string NET_NAME = "name";
    public const string NET_PROTOTYPE = "prototype";
    public const string NET_IS_ALIVE = "alive";
    public const string NET_IS_ACTIVE = "acive";
    public const string NET_IS_PANEL_LOCKED = "locked";
    public const string NET_ENERGY = "energy";
    public const string NET_MAX_ENERGY = "maxenergy";
    public const string NET_COORDINATES = "coords";
    public const string NET_AVAILABLE_ACTION = "actions";
    public const string NET_FREEZE = "freeze";

    ///Used by the CyborgMonitoringServerSystem to send the status of all connected cyborg sensors to each crew monitor
    public const string NET_STATUS_COLLECTION = "suit-status-collection";
}
