using Content.Server.UserInterface;
using Content.Shared.White.Cult.UI;
using Robust.Server.GameObjects;

namespace Content.Shared.White.Cult;

[RegisterComponent]
public sealed class RuneDrawerProviderComponent : Component
{
    [ViewVariables]
    public BoundUserInterface? UserInterface => Owner.GetUIOrNull(ListViewSelectorUiKey.Key);

    [DataField("runePrototypes")]
    public List<string> RunePrototypes = new();
}
