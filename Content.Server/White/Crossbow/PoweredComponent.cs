using System.Numerics;
using Content.Shared.Damage;

namespace Content.Server.White.Crossbow;

[RegisterComponent]
public sealed class PoweredComponent : Component
{
    [DataField("charge", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Charge;

    [DataField("damage", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = new();
}
