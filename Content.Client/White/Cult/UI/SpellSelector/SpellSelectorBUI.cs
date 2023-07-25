using Content.Client.White.UserInterface.Controls;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Components;
using Robust.Client.GameObjects;
using Robust.Client.Utility;

namespace Content.Client.White.Cult.UI.SpellSelector;

public sealed class SpellSelectorBUI : BoundUserInterface
{

    private RadialContainer? _radialContainer;

    public SpellSelectorBUI(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();
        _radialContainer = new RadialContainer();
        _radialContainer.Closed += Close;

        foreach (var action in CultistComponent.CultistActions)
        {
            var button = _radialContainer.AddButton(action.DisplayName, action.Icon?.Frame0());

            button.Controller.OnPressed += _ =>
            {
                SendMessage(new CultEmpowerSelectedBuiMessage(action));
                Close();
            };
        }

        _radialContainer.OpenAttachedLocalPlayer();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _radialContainer?.Close();
    }
}
