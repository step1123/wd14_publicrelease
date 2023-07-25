using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.White.Cyborg.CyborgSensor;


[RegisterComponent]
public sealed class CyborgSensorComponent : Component
{
    /// <summary>
    ///     How often does sensor update its owners status (in seconds). Limited by the system update rate.
    /// </summary>
    [DataField("updateRate")]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(2f);

    /// <summary>
    ///     Next time when sensor updated owners status
    /// </summary>
    [DataField("nextUpdate", customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    ///     The station this suit sensor belongs to. If it's null the suit didn't spawn on a station and the sensor doesn't work.
    /// </summary>
    [DataField("station")]
    public EntityUid? StationId = null;

    /// <summary>
    ///     The server the suit sensor sends it state to.
    ///     The suit sensor will try connecting to a new server when no server is connected.
    ///     It does this by calling the servers entity system for performance reasons.
    /// </summary>
    [DataField("server")]
    public string? ConnectedServer = null;
}
