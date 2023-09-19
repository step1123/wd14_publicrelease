namespace Content.Server.Animations;

[RegisterComponent]
public sealed class DancingComponent : Component
{
    public float AccumulatedFrametime;

    public float NextDelay;
}
