using Content.Shared.White.Cyborg.CyborgSensor;

namespace Content.Server.White.Cyborg.CyborgMonitoring;


[RegisterComponent]
public sealed class CyborgMonitoringConsoleComponent : Component
{
    public Dictionary<string,CyborgSensorStatus> ConnectedSensors = new();
}



