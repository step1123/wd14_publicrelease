using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable, NetSerializable]
public sealed class ToolUseOnBorgEvent : HandledEntityEventArgs
{
    public EntityUid CyborgUid;
    public EntityUid Used;
    public EntityUid User;

    public ToolUseOnBorgEvent(EntityUid cyborgUid, EntityUid used, EntityUid user)
    {
        CyborgUid = cyborgUid;
        Used = used;
        User = user;
    }
}
