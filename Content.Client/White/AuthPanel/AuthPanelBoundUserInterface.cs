using Content.Shared.White.AuthPanel;
using Robust.Client.GameObjects;

namespace Content.Client.White.AuthPanel;

public sealed class AuthPanelBoundUserInterface : BoundUserInterface
{
    private AuthPanelMenu? _menu;

    public AuthPanelBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _menu = new AuthPanelMenu();

        _menu.OnRedButtonPressed(_=>SendButtonPressed(AuthPanelAction.ERTRecruit));
        _menu.OnAccessButtonPressed(_=>SendButtonPressed(AuthPanelAction.AddAccess));
        _menu.OnBluespaceWeaponButtonPressed(_=>SendButtonPressed(AuthPanelAction.BluespaceWeapon));

        _menu.OnClose += Close;
        _menu.OpenCentered();
    }

    public void SendButtonPressed(AuthPanelAction button)
    {
        SendMessage(new AuthPanelButtonPressedMessage(button,_menu?.GetReason()));
    }


    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if(state is not AuthPanelConfirmationActionState confirmationActionState)
            return;

        var action = confirmationActionState.Action;

        if(action.Action is AuthPanelAction.AddAccess)
            _menu?.SetAccessCount(action.ConfirmedPeopleCount,action.MaxConfirmedPeopleCount);
        if(action.Action is AuthPanelAction.ERTRecruit)
            _menu?.SetRedCount(action.ConfirmedPeopleCount,action.MaxConfirmedPeopleCount);
        if(action.Action is AuthPanelAction.BluespaceWeapon)
            _menu?.SetWeaponCount(action.ConfirmedPeopleCount,action.MaxConfirmedPeopleCount);

        _menu?.SetReason(action.Reason);

    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Close();
    }
}
