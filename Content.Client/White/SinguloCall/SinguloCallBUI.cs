using Content.Shared.White.Cult.UI;
using Robust.Client.GameObjects;

namespace Content.Client.White.SinguloCall;

public sealed class SinguloCallBUI : BoundUserInterface
{
    public SinguloCallBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    private SinguloCall? _window;

    protected override void Open()
    {
        base.Open();

        _window = new();
        _window.OpenCentered();
        _window.OnNameChange += OnNameSelected;
        _window.OnClose += Close;
    }

    private void OnNameSelected(string name)
    {
        SendMessage(new SinguloCallMessage(name));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not SinguloCallBuiState cast || _window == null)
        {
            return;
        }

        _window.UpdateState(cast.Name);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
