using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable,NetSerializable]
public sealed class CyborgPartGetGibbedEvent : EntityEventArgs
{
    public EntityUid CyborgUid;

    public CyborgPartGetGibbedEvent(EntityUid cyborgUid)
    {
        CyborgUid = cyborgUid;
    }
}
