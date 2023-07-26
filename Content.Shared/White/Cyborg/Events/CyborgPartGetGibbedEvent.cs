using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable]
[NetSerializable]
public sealed class CyborgPartGetGibbedEvent : BaseBorgEvent
{
    public CyborgPartGetGibbedEvent(EntityUid cyborgUid) : base(cyborgUid)
    {
    }
}
