using Robust.Shared.Serialization;

namespace Content.Shared.White.AuthPanel;

[Serializable, NetSerializable]
public enum AuthPanelUiKey
{
    Key,
}

[Serializable, NetSerializable]
public enum AuthPanelAction
{
    ERTRecruit,
    AddAccess,
    BluespaceWeapon
}


[Serializable, NetSerializable]
public sealed class AuthPanelButtonPressedMessage : BoundUserInterfaceMessage
{
    public AuthPanelAction Button;

    public AuthPanelButtonPressedMessage(AuthPanelAction button)
    {
        Button = button;
    }
}

[Serializable, NetSerializable]
public sealed class AuthPanelConfirmationActionState : BoundUserInterfaceState
{
    public HashSet<AuthPanelConfirmationAction> Actions;

    public AuthPanelConfirmationActionState(HashSet<AuthPanelConfirmationAction> actions)
    {
        Actions = actions;
    }
}

[Serializable, NetSerializable]
public sealed class AuthPanelConfirmationAction
{
    public AuthPanelAction Action;
    public int ConfirmedPeopleCount;
    public int MaxConfirmedPeopleCount;

    public AuthPanelConfirmationAction(AuthPanelAction action, int confirmedPeopleCount, int maxConfirmedPeopleCount)
    {
        Action = action;
        ConfirmedPeopleCount = confirmedPeopleCount;
        MaxConfirmedPeopleCount = maxConfirmedPeopleCount;
    }
}

[Serializable, NetSerializable]
public sealed class AuthPanelPerformActionEvent : EntityEventArgs
{
    public AuthPanelAction Action;

    public AuthPanelPerformActionEvent(AuthPanelAction action)
    {
        Action = action;
    }
}


[Serializable, NetSerializable]
public enum AuthPanelVisualLayers : byte
{
   Confirm
}
