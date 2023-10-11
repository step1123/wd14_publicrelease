﻿namespace Content.Server.White.SharpeningSystem;

[RegisterComponent]
public sealed class SharpenerComponent : Component
{
    //rn gonna support only slash damage
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("damageModifier")]
    public int DamageModifier;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("usages")]
    public int Usages = 1;
}

[RegisterComponent]
public sealed class SharpenedComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public int DamageModifier = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public int AttacksLeft = 50;
}
