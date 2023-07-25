using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;

namespace Content.Server.White.BecomeDustOnDeathSystem;

[RegisterComponent]
public sealed class BecomeDustOnDeathComponent : Component
{
    [DataField("sprite", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string SpawnOnDeathPrototype = "Ectoplasm";
}
