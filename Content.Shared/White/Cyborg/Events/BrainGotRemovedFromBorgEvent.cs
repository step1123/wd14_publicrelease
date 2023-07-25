namespace Content.Shared.White.Cyborg.Events;

public sealed class BrainGotRemovedFromBorgEvent: EntityEventArgs
{
    public EntityUid CyborgUid;

    public BrainGotRemovedFromBorgEvent(EntityUid cyborgUid)
    {
        CyborgUid = cyborgUid;
    }
}
