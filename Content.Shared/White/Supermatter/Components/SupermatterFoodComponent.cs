namespace Content.Shared.White.Supermatter.Components;

[RegisterComponent]
public sealed class SupermatterFoodComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("energy")]
    public int Energy { get; set; } = 1;
}
