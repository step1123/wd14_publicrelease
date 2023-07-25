namespace Content.Shared.White.Cyborg.Events;

public sealed class CyborgActionSelectedEvent : EntityEventArgs
{
    public Enum Action;
    public EntityUid ActionSelector;
    public EntityUid? User;

    public CyborgActionSelectedEvent(Enum action, EntityUid actionSelector,EntityUid? user)
    {
        Action = action;
        ActionSelector = actionSelector;
        User = user;
    }
}
