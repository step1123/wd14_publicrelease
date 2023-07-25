namespace Content.Server.White.Cult.Runes.Comps;

[RegisterComponent]
public sealed class CultRuneBuffComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("rangeTarget")]
    public float RangeTarget = 0.3f;
}
