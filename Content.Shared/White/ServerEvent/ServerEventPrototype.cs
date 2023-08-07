using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.ServerEvent;

[Prototype("serverEvent")]
public sealed class ServerEventPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("name")]
    public string Name { get; } = string.Empty;

    [DataField("description")]
    public string Description { get; } = string.Empty;

    [DataField("playerGatherTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan PlayerGatherTime = TimeSpan.FromMinutes(1);

    [ViewVariables] public TimeSpan? CurrentPlayerGatherTime = null;
    [ViewVariables] public TimeSpan? EndPlayerGatherTime = null;
}
