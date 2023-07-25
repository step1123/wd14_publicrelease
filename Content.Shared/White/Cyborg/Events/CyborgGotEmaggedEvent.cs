using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable, NetSerializable]
public sealed class CyborgGotEmaggedEvent : EntityEventArgs
{
    public EntityUid User;

    public CyborgGotEmaggedEvent(EntityUid user)
    {
        User = user;
    }
}
