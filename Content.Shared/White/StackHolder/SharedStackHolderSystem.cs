using Robust.Shared.Serialization;

namespace Content.Shared.White.StackHolder;

public abstract class SharedStackHolderSystem : EntitySystem
{
}

[Serializable]
[NetSerializable]
public sealed class StackChangeEvent : EntityEventArgs
{
    public EntityUid Original;
    public EntityUid Replica;

    public StackChangeEvent(EntityUid original, EntityUid replica)
    {
        Original = original;
        Replica = replica;
    }
}
