using Content.Shared.White.Cult.UI;
using Robust.Client.GameObjects;

namespace Content.Client.White.Cult.UI.SummonCultistList;

public sealed class SummonCultistListWindowBUI : BoundUserInterface
{
    private SummonCultistListWindow? _window;

    public SummonCultistListWindowBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = new();
        _window.OpenCentered();
        _window.OnClose += Close;

        _window.ItemSelected += (item, index) =>
        {
            var msg = new SummonCultistListWindowItemSelectedMessage(item, index);
            SendMessage(msg);
            _window.Close();
        };

        if (State != null)
            UpdateState(State);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is SummonCultistListWindowBUIState newState)
        {
            _window?.PopulateList(newState.Items, newState.Label);
        }
    }
}
