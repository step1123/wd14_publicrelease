using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.White.CustomGhostSystem;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("customGhost")]
public sealed class CustomGhostPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("ckey", required: true)]
    public string Ckey { get; } = default!;

    [DataField("sprite", required: true)]
    public ResPath CustomSpritePath { get; } = default!;

    [DataField("alpha")]
    public float AlphaOverride { get; } = -1;

    [DataField("ghostName")]
    public string GhostName = string.Empty;

    [DataField("ghostDescription")]
    public string GhostDescription = string.Empty;
}

[Serializable, NetSerializable]
public enum CustomGhostAppearance
{
    Sprite,
    AlphaOverride
}
