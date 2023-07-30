using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.Laws;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.White.Cyborg.CyborgConsole;

[UsedImplicitly]
public sealed class CyborgConsoleBoundUserInterface : BoundUserInterface
{
    private CyborgConsoleMenu? _menu;

    public CyborgConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = new CyborgConsoleMenu(this);
        _menu.OpenCenteredLeft();
        _menu.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is CyborgMonitoringState monitoringState)
            _menu?.UpdateCyborgsList(monitoringState.Sensors);
        if (state is LawsUpdateState lawsState)
            _menu?.SetUnitInformation(new CyborgConsoleLawsControl(lawsState.Uid, lawsState.Laws, this));
    }


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Dispose();
    }

    public void SendActionMessage(string address, Enum action)
    {
        SendMessage(new CyborgActionMessage(action, address));
    }

    public void SendAddLawMessage(EntityUid uid, string law, int? index = null)
    {
        SendMessage(new AddLawMessage(uid, law, index));
    }

    public void SendReIndexLawMessage(EntityUid uid, int index, int newIndex)
    {
        SendMessage(new ReIndexLawMessage(uid, index, newIndex));
    }

    public void SendRemoveLawMessage(EntityUid uid, int index)
    {
        SendMessage(new RemoveLawMessage(uid, index));
    }
}
