namespace Content.Server.White.Other.ChangeThrowForceSystem;

[RegisterComponent]
public sealed class ChangeThrowForceComponent : Component
{
    [DataField("throwForce")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float ThrowForce = 1f;
}
