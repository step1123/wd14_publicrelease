using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable, NetSerializable]
public sealed class BrainGotInsertedToBorgEvent: EntityEventArgs
{
    public EntityUid CyborgUid;

    public BrainGotInsertedToBorgEvent(EntityUid cyborgUid)
    {
        CyborgUid = cyborgUid;
    }
}
