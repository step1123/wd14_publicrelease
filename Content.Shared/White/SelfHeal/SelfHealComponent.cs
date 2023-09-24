using Content.Shared.Actions.ActionTypes;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.White.SelfHeal;

[RegisterComponent]
public sealed class SelfHealComponent: Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("delay")]
    public float Delay = 3f;

    [ViewVariables(VVAccess.ReadWrite), DataField("healingSound")]
    public SoundSpecifier? HealingSound;

    [ViewVariables(VVAccess.ReadWrite), DataField("damage", required: true)]
    public DamageSpecifier Damage = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("damageContainers", customTypeSerializer: typeof(PrototypeIdListSerializer<DamageContainerPrototype>))]
    public List<string>? DamageContainers;

    [ViewVariables(VVAccess.ReadWrite),DataField("disallowedClothingUser")]
    public List<string>? DisallowedClothingUser;

    [ViewVariables(VVAccess.ReadWrite), DataField("disallowedClothingTarget")]
    public List<string>? DisallowedClothingTarget;

    [ViewVariables(VVAccess.ReadWrite), DataField("action")]
    public TargetedAction? Action;
}
