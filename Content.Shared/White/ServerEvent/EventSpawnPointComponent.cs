using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.ServerEvent;

[RegisterComponent]
public sealed class EventSpawnPointComponent : Component
{
    [DataField("prototype", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>),required:true)]
    public string EntityPrototype = default!;

    [DataField("event", customTypeSerializer:typeof(PrototypeIdSerializer<ServerEventPrototype>))]
    public string EventType = "none";
}
