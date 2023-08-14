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
    public string? Reason;

    public AuthPanelButtonPressedMessage(AuthPanelAction button, string? reason)
    {
        Button = button;
        Reason = reason;
    }
}

[Serializable, NetSerializable]
public sealed class AuthPanelConfirmationActionState : BoundUserInterfaceState
{
    public AuthPanelConfirmationAction Action;

    public AuthPanelConfirmationActionState(AuthPanelConfirmationAction action)
    {
        Action = action;
    }
}

[Serializable, NetSerializable]
public sealed class AuthPanelConfirmationAction
{
    public AuthPanelAction Action;
    public int ConfirmedPeopleCount;
    public int MaxConfirmedPeopleCount;
    public string Reason;

    public AuthPanelConfirmationAction(AuthPanelAction action, int confirmedPeopleCount, int maxConfirmedPeopleCount, string reason)
    {
        Action = action;
        ConfirmedPeopleCount = confirmedPeopleCount;
        MaxConfirmedPeopleCount = maxConfirmedPeopleCount;
        Reason = reason;
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
