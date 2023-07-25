namespace Content.Server.White.Cult.Runes.Comps;

[RegisterComponent]
public sealed class CultRuneTeleportComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("rangeTarget")]
    public float RangeTarget = 0.3f;

    [ViewVariables(VVAccess.ReadWrite), DataField("label")]
    public string? Label;
}
