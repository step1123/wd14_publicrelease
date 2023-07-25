using Content.Server.UserInterface;
using Content.Shared.White.Cult.Items;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cult.Items.Components;

[RegisterComponent]
public sealed class TorchCultistsProviderComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public BoundUserInterface? UserInterface => Owner.GetUIOrNull(CultTeleporterUiKey.Key);

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ItemSelected;

    [ViewVariables(VVAccess.ReadWrite), DataField("cooldown")]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);

    [ViewVariables(VVAccess.ReadWrite), DataField("usesLeft")]
    public int UsesLeft = 3;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextUse = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool Active = true;
}
