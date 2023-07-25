namespace Content.Server.White.Cult.Runes.Comps;

[RegisterComponent]
public sealed class CultRuneReviveComponent: Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("usesToRevive")]
    public uint UsesToRevive = 3;

    [ViewVariables(VVAccess.ReadWrite), DataField("usesHave")]
    public uint UsesHave = 3;

    [ViewVariables(VVAccess.ReadWrite), DataField("rangeTarget")]
    public float RangeTarget = 0.3f;
}
