namespace Content.Server.Worldgen;


[RegisterComponent]
public sealed class StructurePlacementComponent : Component
{
    /// <summary>
    /// The structures to place into the world.
    /// </summary>
    [DataField("structures")] public List<StructureConfig> Structures = new();

    /// <summary>
    /// Radius around which there should be no objects for placement to succeed.
    /// </summary>
    [DataField("safetyRadius")] public int SafetyRadius = 128;

    [DataField("placementRadius")] public int PlacementRadius = 4096;
}

/// <summary>
/// A single structure's placement config.
/// </summary>
[DataDefinition]
public sealed class StructureConfig
{
    /// <summary>
    /// The entity to spawn into the world.
    /// </summary>
    [DataField("entity", required: true)] public string Entity = default!;

    /// <summary>
    /// The amount of that entity to spawn.
    /// </summary>
    [DataField("amountRange")] public Vector2i AmountRange = Vector2i.One;
}
