using Content.Client.VendingMachines.UI;
using Content.Shared.VendingMachines;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using System.Linq;
using Content.Client.White.Economy.Ui;

namespace Content.Client.VendingMachines
{
    public sealed class VendingMachineBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VendingMenu? _menu; // WD EDIT

        [ViewVariables]
        private List<VendingMachineInventoryEntry> _cachedInventory = new();

        public VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            var vendingMachineSys = EntMan.System<VendingMachineSystem>();

            // WD EDIT START
            var component = EntMan.GetComponent<VendingMachineComponent>(Owner);
            _cachedInventory = vendingMachineSys.GetAllInventory(Owner, component);

            _menu = new VendingMenu { Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName };

            _menu.OnClose += Close;
            _menu.OnItemSelected += OnItemSelected;
            _menu.OnWithdraw += SendMessage;
            // WD EDIT END

            _menu.Populate(_cachedInventory, component.PriceMultiplier, component.Credits);

            _menu.OpenCentered();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not VendingMachineInterfaceState newState)
                return;

            _cachedInventory = newState.Inventory;

            _menu?.Populate(_cachedInventory, newState.PriceMultiplier, newState.Credits);
        }

        // WD EDIT START
        private void OnItemSelected(int index)
        {
            if (_cachedInventory.Count == 0)
                return;

            var selectedItem = _cachedInventory.ElementAtOrDefault(index);

            if (selectedItem == null)
                return;

            SendMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
        }
        // WD EDIT END

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_menu == null)
                return;

            _menu.OnItemSelected -= OnItemSelected;
            _menu.OnClose -= Close;
            _menu.Dispose();
        }
    }
}
