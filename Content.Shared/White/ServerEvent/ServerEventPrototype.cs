using Content.Shared.White.ServerEvent.Data;
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
    public string Description = string.Empty;

    [DataField("playerGatherTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan PlayerGatherTime = TimeSpan.FromSeconds(10);

    [DataField("onStart")] public IEventAction? OnStart;
    [DataField("onEnd")] public IEventAction? OnEnd;

    [DataField("minPlayer")] public int MinPlayer;

    [ViewVariables] public TimeSpan? CurrentPlayerGatherTime = null;
    [ViewVariables] public TimeSpan? EndPlayerGatherTime = null;

    [ViewVariables] public bool IsBreak;
}
