using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cult.Pylon;

[RegisterComponent]
public sealed class SharedPylonComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("healingAuraRange")]
    public readonly float HealingAuraRange = 5f;

    [ViewVariables(VVAccess.ReadOnly), DataField("healingAuraDamage", required: true)]
    public readonly DamageSpecifier HealingAuraDamage = default!;

    [ViewVariables(VVAccess.ReadOnly), DataField("burnDamageOnInteract", required: true)]
    public readonly DamageSpecifier BurnDamageOnInteract = default!;

    [DataField("burnHandSound")]
    public SoundSpecifier BurnHandSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("healingAuraCooldown")]
    public readonly float HealingAuraCooldown = 5f;

    public TimeSpan NextHealTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly), DataField("activated")]
    public bool Activated = true;

    [ViewVariables(VVAccess.ReadWrite), DataField("tilesConvertRange")]
    public readonly float TileConvertRange = 5f;

    [ViewVariables(VVAccess.ReadOnly), DataField("tileId")]
    public readonly string TileId = "CultFloor";

    [ViewVariables(VVAccess.ReadWrite), DataField("tileConvertCooldown")]
    public readonly float TileConvertCooldown = 15f;

    public TimeSpan NextTileConvert = TimeSpan.Zero;

    [DataField("convertTileSound")]
    public SoundSpecifier ConvertTileSound = new SoundPathSpecifier("/Audio/White/Cult/curse.ogg");

    [ViewVariables(VVAccess.ReadOnly), DataField("tileConvertEffect")]
    public string TileConvertEffect = "CultTileSpawnEffect";

    [ViewVariables(VVAccess.ReadOnly), DataField("bleedReductionAmount")]
    public float BleedReductionAmount = 1.0f;

    [ViewVariables(VVAccess.ReadOnly), DataField("bloodRefreshAmount")]
    public float BloodRefreshAmount = 1.0f;

    [ViewVariables(VVAccess.ReadOnly), DataField("convertEverything")]
    public bool ConvertEverything;

    [ViewVariables(VVAccess.ReadOnly), DataField("wallId")]
    public string WallId = "WallCult";

    [ViewVariables(VVAccess.ReadOnly), DataField("airlockId")]
    public string AirlockId = "AirlockGlassCult";

    [DataField("airlockConvertEffect")]
    public string AirlockConvertEffect = "CultAirlockGlow";

    [DataField("wallConvertEffect")]
    public string WallConvertEffect = "CultWallGlow";
}

[Serializable, NetSerializable]
public enum PylonVisuals : byte
{
    Activated
}
