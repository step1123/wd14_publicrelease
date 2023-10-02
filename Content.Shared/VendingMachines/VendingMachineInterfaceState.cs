using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines
{
    [NetSerializable, Serializable]
    public sealed class VendingMachineInterfaceState : BoundUserInterfaceState
    {
        public List<VendingMachineInventoryEntry> Inventory;
        // WD EDIT START
        public double PriceMultiplier;
        public int Credits;

        public VendingMachineInterfaceState(List<VendingMachineInventoryEntry> inventory, double priceMultiplier,
            int credits)
        {
            Inventory = inventory;
            PriceMultiplier = priceMultiplier;
            Credits = credits;
        }
    }

    [Serializable, NetSerializable]
    public sealed class VendingMachineWithdrawMessage : BoundUserInterfaceMessage
    {
    }
    // WD EDIT END

    [Serializable, NetSerializable]
    public sealed class VendingMachineEjectMessage : BoundUserInterfaceMessage
    {
        public readonly InventoryType Type;
        public readonly string ID;
        public VendingMachineEjectMessage(InventoryType type, string id) // WD EDIT
        {
            Type = type;
            ID = id;
        }
    }

    [Serializable, NetSerializable]
    public enum VendingMachineUiKey
    {
        Key,
    }
}
