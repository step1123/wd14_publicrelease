using Content.Shared.Damage;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class CyborgZapModuleComponent : Component
{
    [ViewVariables] public bool IsUsed;
    [DataField("zapHeal", required: true), ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier ZapHeal = default!;
}
