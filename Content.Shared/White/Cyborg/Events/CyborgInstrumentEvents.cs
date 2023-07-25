using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable,NetSerializable]
public sealed class CyborgInstrumentGotInsertedEvent : EntityEventArgs
{
    public EntityUid CyborgUid;

    public CyborgInstrumentGotInsertedEvent(EntityUid uid)
    {
        CyborgUid = uid;
    }
}

[Serializable,NetSerializable]
public sealed class CyborgInstrumentGotPickupEvent : EntityEventArgs
{
    public EntityUid CyborgUid;

    public CyborgInstrumentGotPickupEvent(EntityUid uid)
    {
        CyborgUid = uid;
    }
}
