using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable]
[NetSerializable]
public abstract class BaseBorgEvent : EntityEventArgs
{
    public EntityUid CyborgUid;

    public BaseBorgEvent(EntityUid cyborgUid)
    {
        CyborgUid = cyborgUid;
    }
}
