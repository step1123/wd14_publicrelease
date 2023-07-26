using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.Cyborg;

[Prototype("cyborg")]
public sealed class CyborgPrototype : IPrototype
{
    [DataField("description", required: true)]
    public string CyborgDescription = string.Empty;

    [DataField("name", required: true)] public string CyborgName = string.Empty;

    [DataField("cyborgPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>), required: true)]
    public string CyborgSpawnPrototype = default!;

    [DataField("icon")] public string Icon { get; } = default!;

    [IdDataField] public string ID { get; } = default!;
}
