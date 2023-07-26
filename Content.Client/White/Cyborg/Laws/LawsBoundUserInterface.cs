using Content.Shared.White.Cyborg.Laws;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.White.Cyborg.Laws;

[UsedImplicitly]
public sealed class LawsBoundUserInterface : BoundUserInterface
{
    private LawsMenu? _lawsMenu;

    public EntityUid Machine;

    public LawsBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
    {
        Machine = owner.Owner;
    }

    protected override void Open()
    {
        base.Open();

        _lawsMenu = new LawsMenu(this);

        _lawsMenu.OnClose += Close;

        _lawsMenu.OpenCentered();
    }

    public void StateLawsMessage(List<string> laws)
    {
        SendMessage(new StateLawsMessage(laws));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not LawsUpdateState lawsUpdateState)
            return;
        _lawsMenu?.UpdateLaws();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _lawsMenu?.Dispose();
    }
}
