using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable, NetSerializable]
public sealed class ModuleGotEmaggedEvent : EntityEventArgs
{
    public EntityUid User;
    public EntityUid Cyborg;

    public ModuleGotEmaggedEvent(EntityUid user, EntityUid cyborg)
    {
        User = user;
        Cyborg = cyborg;
    }
}
