using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

public abstract class ModuleChangedEvent : EntityEventArgs
{
    public EntityUid CyborgUid;
    public EntityUid ModuleUid;

    public ModuleChangedEvent(EntityUid moduleUid, EntityUid cyborgUid)
    {
        ModuleUid = moduleUid;
        CyborgUid = cyborgUid;
    }
}

public sealed class ModuleInsertEvent : ModuleChangedEvent
{
    public ModuleInsertEvent(EntityUid moduleUid, EntityUid cyborgUid) : base(moduleUid, cyborgUid)
    {
    }
}

public sealed class ModuleRemoveEvent : ModuleChangedEvent
{
    public ModuleRemoveEvent(EntityUid moduleUid, EntityUid cyborgUid) : base(moduleUid, cyborgUid)
    {
    }
}

[Serializable]
[NetSerializable]
public sealed class ModuleGotEmaggedEvent : EntityEventArgs
{
    public EntityUid Cyborg;
    public EntityUid User;

    public ModuleGotEmaggedEvent(EntityUid user, EntityUid cyborg)
    {
        User = user;
        Cyborg = cyborg;
    }
}
