using Robust.Shared.Containers;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class CyborgInstrumentModuleComponent : Component
{
    [ViewVariables]
    public Container InstrumentContainer = default!;
    public const string InstrumentContainerName = "instrument_slots";

    [ViewVariables]
    public HashSet<EntityUid> InstrumentUids = new();

}
