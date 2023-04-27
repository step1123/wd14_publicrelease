using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.White.NonPeacefulRoundEnd;

[Prototype("nonPeacefulRoundEndItems")]
public sealed class NonPeacefulRoundItemsPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField("items", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> Items { get; } = default!;
}
