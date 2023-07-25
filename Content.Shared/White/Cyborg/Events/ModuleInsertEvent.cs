using Content.Shared.White.Cyborg.Components;

namespace Content.Shared.White.Cyborg.Events;

public sealed class ModuleInsertEvent : ModuleChangedEvent
{
    public ModuleInsertEvent(EntityUid moduleUid , EntityUid cyborgUid) : base(moduleUid, cyborgUid)
    {
    }
}
