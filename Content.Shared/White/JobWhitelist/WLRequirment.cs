using Content.Shared.Roles;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.JobWhitelist;

public sealed class WLRequirement : JobRequirement
{
    [DataField("requirementJob", customTypeSerializer: typeof(PrototypeIdSerializer<JobPrototype>))]
    public string Key = default!;
}
