using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable,NetSerializable]
public sealed class BrainInsertedToMMIEvent : CancellableEntityEventArgs
{
    public EntityUid BrainUid;

    public BrainInsertedToMMIEvent(EntityUid brainUid)
    {
        BrainUid = brainUid;
    }
}
