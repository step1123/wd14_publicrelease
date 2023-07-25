namespace Content.Server.White.Cult.Runes.Comps;

[RegisterComponent]
public sealed class CultTeleportRuneProviderComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Target;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? BaseRune;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid>? Targets;
}
