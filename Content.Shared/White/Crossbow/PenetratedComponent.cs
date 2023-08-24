namespace Content.Shared.White.Crossbow;

[RegisterComponent]
public sealed class PenetratedComponent : Component
{
    public EntityUid? ProjectileUid;

    public bool IsPinned;
}
