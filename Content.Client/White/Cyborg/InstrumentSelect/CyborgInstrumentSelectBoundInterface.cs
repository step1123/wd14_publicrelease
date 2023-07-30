using Content.Shared.White.Cyborg;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.White.Cyborg.InstrumentSelect;

[UsedImplicitly]
public sealed class CyborgInstrumentSelectBoundInterface : BoundUserInterface
{
    private CyborgInstrumentSelectMenu? _menu;
    public EntityUid Machine;

    public CyborgInstrumentSelectBoundInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        Machine = owner;
    }

    protected override void Open()
    {
        base.Open();
        _menu = new CyborgInstrumentSelectMenu(this);
        _menu.OnClose += Close;
        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not CyborgInstrumentSelectListState selectListState)
            return;
        _menu?.UpdateInstruments(selectListState.Instruments);
        _menu?.UpdateEnergyPanel();
    }

    public void SelectInstrument(EntityUid selectedInstrumentUid)
    {
        SendMessage(new CyborgInstrumentSelectedMessage(selectedInstrumentUid));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Dispose();
    }
}
