using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio;

namespace Content.Shared.White.Crossbow;

[RegisterComponent]
public sealed class DrawableComponent : Component
{
    [ViewVariables]
    public bool Drawn;

    public BallisticAmmoProviderComponent Provider = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("soundInsert")]
    public SoundSpecifier? SoundDraw = new SoundPathSpecifier("/Audio/Weapons/drawbow2.ogg");
}
