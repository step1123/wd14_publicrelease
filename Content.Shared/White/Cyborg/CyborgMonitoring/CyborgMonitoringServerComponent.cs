using Content.Shared.White.Cyborg.CyborgSensor;

namespace Content.Shared.White.Cyborg.CyborgMonitoring;

[RegisterComponent]
public sealed class CyborgMonitoringServerComponent : Component
{
    /// <summary>
    ///     List of all currently connected sensors to this server.
    /// </summary>
    public readonly Dictionary<string, CyborgSensorStatus> SensorStatus = new();


    /// <summary>
    ///     Whether the server is the currently active server for the station it's on
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public bool Active = true;

    /// <summary>
    ///     Whether the server can become the currently active server. The server being unavailable usually means that it isn't
    ///     powered
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public bool Available = true;

    /// <summary>
    ///     After what time sensor consider to be lost.
    /// </summary>
    [DataField("sensorTimeout")] [ViewVariables(VVAccess.ReadWrite)]
    public float SensorTimeout = 10f;
}
