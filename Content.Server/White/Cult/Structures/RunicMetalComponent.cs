using Content.Server.UserInterface;
using Content.Shared.White.Cult.Structures;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cult.Structures;

[RegisterComponent]
public sealed class RunicMetalComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public BoundUserInterface? UserInterface => Owner.GetUIOrNull(CultStructureCraftUiKey.Key);

    [ViewVariables(VVAccess.ReadWrite), DataField("delay")]
    public float Delay = 1;
}
