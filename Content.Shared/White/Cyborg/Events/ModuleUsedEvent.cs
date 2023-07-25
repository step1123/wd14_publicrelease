using Content.Shared.DoAfter;
using Content.Shared.White.Cyborg.Components;

namespace Content.Shared.White.Cyborg.Events;

public sealed class ModuleUsedEvent : EntityEventArgs
{
    public bool Cancelled;
    public CyborgComponent CyborgComponent;

    public ModuleUsedEvent(CyborgComponent cyborgComponent, bool cancelled = false)
    {
        CyborgComponent = cyborgComponent;
        Cancelled = cancelled;
    }
}
