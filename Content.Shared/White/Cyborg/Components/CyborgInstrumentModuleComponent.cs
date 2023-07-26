using Robust.Shared.Containers;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class CyborgInstrumentModuleComponent : Component
{
    public const string InstrumentContainerName = "instrument_slots";

    [ViewVariables] public Container InstrumentContainer = default!;

    [ViewVariables] public HashSet<EntityUid> InstrumentUids = new();
}
