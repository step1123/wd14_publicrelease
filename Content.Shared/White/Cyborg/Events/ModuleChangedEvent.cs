using Content.Shared.White.Cyborg.Components;

namespace Content.Shared.White.Cyborg.Events;

public abstract class ModuleChangedEvent : EntityEventArgs
{
    public EntityUid CyborgUid;
    public EntityUid ModuleUid;
    public ModuleChangedEvent(EntityUid moduleUid,EntityUid cyborgUid)
    {
        ModuleUid = moduleUid;
        CyborgUid = cyborgUid;
    }
}
