using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class ActiveAmmoFillerComponent : Component
{
    [DataField("energyCost")] public float EnergyCost = 8f;

    [DataField("nextUpdate", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime;

    [ViewVariables(VVAccess.ReadWrite)] public TimeSpan UpdateRate = TimeSpan.FromSeconds(1);
}
