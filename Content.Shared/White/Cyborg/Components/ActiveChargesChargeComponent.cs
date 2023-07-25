using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class ActiveChargesChargeComponent : Component
{
    [DataField("nextUpdate", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(1);
}
