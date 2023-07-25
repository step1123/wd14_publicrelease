using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Overlays;

[RegisterComponent, NetworkedComponent]
public sealed class NightVisionComponent : Component
{
    [DataField("tint"), ViewVariables(VVAccess.ReadWrite)]
    public Vector3 Tint = new(0.3f, 0.3f, 0.3f);

    [DataField("strength"), ViewVariables(VVAccess.ReadWrite)]
    public float Strength = 2f;

    [DataField("noise"), ViewVariables(VVAccess.ReadWrite)]
    public float Noise = 0.5f;
}
