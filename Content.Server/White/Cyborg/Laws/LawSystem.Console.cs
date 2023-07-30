using Content.Server.White.Cyborg.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.CyborgMonitoring;
using Content.Shared.White.Cyborg.Events;
using Content.Shared.White.Cyborg.Laws;
using Content.Shared.White.Cyborg.Laws.Component;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cyborg.Laws;

public sealed class LawSystemConsole : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly LawsSystem _law = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgMonitoringConsoleComponent, AddLawMessage>(OnLawAddByConsole);
        SubscribeLocalEvent<CyborgMonitoringConsoleComponent, RemoveLawMessage>(OnLawRemoveByConsole);
        SubscribeLocalEvent<CyborgMonitoringConsoleComponent, ReIndexLawMessage>(OnLawReIndexByConsole);

        SubscribeLocalEvent<LawsComponent, CyborgActionSelectedEvent>(OnLawAction);
    }

    private void OnLawReIndexByConsole(EntityUid uid, CyborgMonitoringConsoleComponent component,
        ReIndexLawMessage args)
    {
        _law.ReIndexLaw(args.Uid, args.Index, args.NewIndex);
        UpdateConsoleInterface(uid, args.Uid);
    }

    private void OnLawRemoveByConsole(EntityUid uid, CyborgMonitoringConsoleComponent component, RemoveLawMessage args)
    {
        _law.RemoveLaw(args.Uid, args.Index);
        UpdateConsoleInterface(uid, args.Uid);
    }

    private void OnLawAddByConsole(EntityUid uid, CyborgMonitoringConsoleComponent component, AddLawMessage args)
    {
        _law.AddLaw(args.Uid, args.Law, args.Index);
        UpdateConsoleInterface(uid, args.Uid);
    }

    private void OnLawAction(EntityUid uid, LawsComponent component, CyborgActionSelectedEvent args)
    {
        if (args.Action is not CyborgActionKey.LawControl || !_cyborg.HasAccess(uid, args.User) || args.User == null)
            return;

        _adminLog.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(args.User.Value):player} seems to be changing the laws of the Borg {ToPrettyString(uid):player}");

        UpdateConsoleInterface(args.ActionSelector, uid, component);
    }


    public void UpdateConsoleInterface(EntityUid consoleUid, EntityUid uid, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        if (!_ui.TryGetUi(consoleUid, CyborgMonitoringConsoleUiKey.Key, out var ui))
            return;
        var state = new LawsUpdateState(component.Laws, uid);

        UserInterfaceSystem.SetUiState(ui, state);
    }
}
