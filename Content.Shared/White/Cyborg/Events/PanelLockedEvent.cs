using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable, NetSerializable]
public sealed class PanelLockedEvent : EntityEventArgs
{
    public bool Locked;

    public PanelLockedEvent(bool locked)
    {
        Locked = locked;
    }
}
