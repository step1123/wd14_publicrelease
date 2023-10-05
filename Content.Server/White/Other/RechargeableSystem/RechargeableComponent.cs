using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server.White.Other.RechargeableSystem;

[RegisterComponent]
public sealed class RechargeableComponent : Component
{
    [DataField("maxCharge")]
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 MaxCharge = FixedPoint2.New(40);

    [DataField("chargePerSecond")]
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 ChargePerSecond = FixedPoint2.New(1);

    [DataField("rechargeDelay")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float RechargeDelay = 30f;

    [DataField("turnOnFailSound")]
    public SoundSpecifier TurnOnFailSound { get; set; } = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Charge;

    [ViewVariables]
    public bool Discharged;

    public float AccumulatedFrametime;
}
