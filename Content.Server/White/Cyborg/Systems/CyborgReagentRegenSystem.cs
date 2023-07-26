using Content.Server.White.Chemistry;
using Content.Shared.White.Cyborg.Components;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgReagentRegenSystem : EntitySystem
{
    [Dependency] private readonly CyborgSystem _cyborg = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgInstrumentComponent, ReagentWillAddedEvent>(OnReagentWillAdded);
        SubscribeLocalEvent<CyborgInstrumentComponent, ReagentAddedEvent>(OnReagentAdded);
    }

    private void OnReagentWillAdded(EntityUid uid, CyborgInstrumentComponent component, ReagentWillAddedEvent args)
    {
        args.Handled = true;
    }

    private void OnReagentAdded(EntityUid uid, CyborgInstrumentComponent component, ReagentAddedEvent args)
    {
        _cyborg.TryChangeEnergy(component.CyborgUid, -args.Accepted * 10);
    }
}
