using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cult;

[Prototype("cultistFactoryProduction")]
public sealed class CultistFactoryProductionPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("item", required: true)]
    public List<string> Item = default!;

    [DataField("icon", required: true)]
    public string Icon = default!;

    [DataField("name", required: true)]
    public string Name = default!;
}

[Serializable, NetSerializable]
public enum CultCraftStructureVisuals : byte
{
    Activated
}
