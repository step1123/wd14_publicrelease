namespace Content.Shared.Damage.Components
{
    [RegisterComponent]
    public sealed class StaminaProtectionComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), DataField("damageReduction")]
        public float DamageReduction { get; set; } = 0.3f; // 30% damage reduction by default.
    }
}
