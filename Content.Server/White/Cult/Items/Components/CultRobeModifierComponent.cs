namespace Content.Server.White.Cult.Items.Components;

[RegisterComponent]
public sealed class CultRobeModifierComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("speedModifier")]
    public float SpeedModifier = 1.45f;

    [ViewVariables(VVAccess.ReadOnly), DataField("damageModifierSetId")]
    public string DamageModifierSetId = "CultRobe";

    public string? StoredDamageSetId { get; set; }
}
