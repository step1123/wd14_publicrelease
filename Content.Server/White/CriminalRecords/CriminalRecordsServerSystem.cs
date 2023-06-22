using Content.Shared.White.CriminalRecords;
using Robust.Server.GameStates;
using Robust.Shared.GameStates;

namespace Content.Server.White.CriminalRecords;

public sealed class CriminalRecordsServerSystem : EntitySystem
{
    [Dependency] private readonly PvsOverrideSystem _pvsSys = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CriminalRecordsServerComponent, ComponentStartup>(OnCompInit);
        SubscribeLocalEvent<CriminalRecordsServerComponent, ComponentRemove>(OnCompRemove);
    }

    private void OnCompInit(EntityUid uid, CriminalRecordsServerComponent component, ComponentStartup args)
    {
        _pvsSys.AddGlobalOverride(uid);
    }

    private void OnCompRemove(EntityUid uid, CriminalRecordsServerComponent component, ComponentRemove args)
    {
        _pvsSys.ClearOverride(uid);
    }
}
