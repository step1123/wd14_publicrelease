namespace Content.Server.White.Cult.Runes.Comps;

[RegisterComponent]
public sealed class CultRuneSummoningProviderComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? BaseRune;
}
