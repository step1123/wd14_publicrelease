using Content.Shared.White.Cyborg.CyborgSensor;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg;

[NetSerializable]
[Serializable]
public enum CyborgMonitoringConsoleUiKey : byte
{
    Key
}

[NetSerializable]
[Serializable]
public enum CyborgActionKey : byte
{
    LawControl,
    Blow,
    Freeze
}

[NetSerializable]
[Serializable]
public sealed class CyborgMonitoringState : BoundUserInterfaceState
{
    public List<CyborgSensorStatus> Sensors;

    public CyborgMonitoringState(List<CyborgSensorStatus> sensors)
    {
        Sensors = sensors;
    }
}

[NetSerializable]
[Serializable]
public sealed class CyborgActionMessage : BoundUserInterfaceMessage
{
    public Enum Action;
    public string Address;

    public CyborgActionMessage(Enum action, string address)
    {
        Action = action;
        Address = address;
    }
}
