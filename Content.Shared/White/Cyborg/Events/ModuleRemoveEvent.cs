using Content.Shared.White.Cyborg.Components;

namespace Content.Shared.White.Cyborg.Events;

public sealed class ModuleRemoveEvent : ModuleChangedEvent
{
    public ModuleRemoveEvent(EntityUid moduleUid,EntityUid cyborgUid) : base(moduleUid, cyborgUid)
    {
    }
}
