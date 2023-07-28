using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.SiliconBrain;

[Serializable]
[NetSerializable]
public sealed class BrainInsertedToMMIEvent : CancellableEntityEventArgs
{
    public EntityUid BrainUid;

    public BrainInsertedToMMIEvent(EntityUid brainUid)
    {
        BrainUid = brainUid;
    }
}

[Serializable]
[NetSerializable]
public sealed class BrainInsertEvent : EntityEventArgs
{
    public EntityUid ParentUid;

    public BrainInsertEvent(EntityUid parentUid)
    {
        ParentUid = parentUid;
    }
}

[Serializable]
[NetSerializable]
public sealed class BrainRemoveEvent : EntityEventArgs
{
}

[Serializable]
[NetSerializable]
public sealed class SiliconMindDoAfterEvent : SimpleDoAfterEvent
{
}
