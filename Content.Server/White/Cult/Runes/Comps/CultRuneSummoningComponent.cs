namespace Content.Server.White.Cult.Runes.Comps;

[RegisterComponent]
public sealed class CultRuneSummoningComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("summonMinCount")]
    public uint SummonMinCount = 2;
}
