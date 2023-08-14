using Content.Server.Atmos;

namespace Content.Server.White.Flamethrower;

[RegisterComponent]
public sealed class GasProjectileComponent : Component
{
    public GasMixture? GasMixture;

    public TileInfo? LastTile;

    public TileInfo? CurTile;

    [DataField("gasUsagePerTile", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public float GasUsagePerTile;
}

public record struct TileInfo(EntityUid? GridUid, EntityUid? MapUid, Vector2i Tile);
