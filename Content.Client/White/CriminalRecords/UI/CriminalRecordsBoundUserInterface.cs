using Content.Shared.Containers.ItemSlots;
using Content.Shared.StationRecords;
using Content.Shared.White.CriminalRecords;
using Robust.Client.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Client.White.CriminalRecords.UI;

public sealed class CriminalRecordsBoundUserInterface : BoundUserInterface
{
    private CriminalRecordsWindow? _window = default!;

    public CriminalRecordsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {}

    protected override void Open()
    {
        base.Open();

        _window = new();
        _window.OnKeySelected += OnKeySelected;
        _window.OnStatusSelected += OnStatusSelected;
        _window.OnTextBindDown += OnTextEntered;
        _window.LogOutButton.Controller.OnPressed += _ =>
        {
            SendMessage(new ItemSlotButtonPressedEvent(CriminalRecordsConsoleComponent.IdSlotId));
        };
        _window.NonLogOutButton.Controller.OnPressed += _ =>
        {
            SendMessage(new ItemSlotButtonPressedEvent(CriminalRecordsConsoleComponent.IdSlotId));
        };
        _window.LogInButton.Controller.OnPressed += _ =>
        {
            SendMessage(new ItemSlotButtonPressedEvent(CriminalRecordsConsoleComponent.IdSlotId));
        };
        _window.OnClose += Close;

        _window.OpenCentered();
    }

    private void OnKeySelected(StationRecordKey key)
    {
        SendMessage(new SelectCriminalRecord(key));
    }

    private void OnStatusSelected(StationRecordKey key, CriminalRecordInfo status)
    {
        SendMessage(new SelectCriminalStatus(key, status));
    }

    private void OnTextEntered(StationRecordKey key, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            SendMessage(new SelectCriminalReason(key, text));
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CriminalRecordsConsoleBuiState cast)
        {
            return;
        }

        _window?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
        _window?.Dispose();
    }
}
