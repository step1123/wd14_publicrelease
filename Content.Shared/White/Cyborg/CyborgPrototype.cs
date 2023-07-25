using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.Cyborg;

[Prototype("cyborg")]
public sealed class CyborgPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("name",required:true)]
    public string CyborgName = string.Empty;

    [DataField("description",required:true)]
    public string CyborgDescription = string.Empty;

    [DataField("cyborgPolymorph", customTypeSerializer: typeof(PrototypeIdSerializer<PolymorphPrototype>),required:true)]
    public string CyborgPolymorph = default!;

    [DataField("icon")] public string Icon { get; } = default!;

}
