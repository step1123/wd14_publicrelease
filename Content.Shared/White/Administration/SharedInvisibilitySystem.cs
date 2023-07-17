using Robust.Shared.Serialization;

namespace Content.Shared.White.Administration;

public abstract class SharedInvisibilitySystem : EntitySystem
{
}

[Serializable, NetSerializable]
public sealed class InvisibilityToggleEvent : EntityEventArgs
{
    public EntityUid Uid { get; }
    public bool Invisible { get; }

    public InvisibilityToggleEvent(EntityUid uid, bool invisible)
    {
        Uid = uid;
        Invisible = invisible;
    }
}
