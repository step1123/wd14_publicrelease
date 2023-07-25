using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.White.Cult;

[RegisterComponent]
public sealed class ConstructComponent : Component
{
    [DataField("actions", customTypeSerializer: typeof(PrototypeIdListSerializer<InstantActionPrototype>))]
    public List<string> Actions = new();
}
