using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.GhostRecruitment;

[RegisterComponent]
public sealed class GhostRecruitmentSpawnPointComponent : Component
{
    [DataField("prototype", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>),required:true)]
    public string EntityPrototype = default!;

    [DataField("recruitmentName")]
    public string RecruitmentName = "default";

    [DataField("priority")] public int Priority = 5;
}
