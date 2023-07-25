namespace Content.Server.White.IncorporealSystem;

[RegisterComponent]
public sealed class IncorporealComponent : Component
{
    [DataField("movementSpeedBuff")]
    public float MovementSpeedBuff = 1.5f;
}
