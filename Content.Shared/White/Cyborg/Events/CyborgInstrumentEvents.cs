using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable]
[NetSerializable]
public sealed class CyborgInstrumentGotInsertedEvent : BaseBorgEvent
{
    public CyborgInstrumentGotInsertedEvent(EntityUid cyborgUid) : base(cyborgUid)
    {
    }
}

[Serializable]
[NetSerializable]
public sealed class CyborgInstrumentGotPickupEvent : BaseBorgEvent
{
    public CyborgInstrumentGotPickupEvent(EntityUid cyborgUid) : base(cyborgUid)
    {
    }
}
