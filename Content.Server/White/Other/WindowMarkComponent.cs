using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.White.Other;

[RegisterComponent]
public sealed class WindowMarkComponent : Component
{
    [DataField("replacementProto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ReplacementProto = default!;
}
