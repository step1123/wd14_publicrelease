using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines
{
    [NetSerializable, Serializable]
    public sealed class VendingMachineInterfaceState : BoundUserInterfaceState
    {
        // WD EDIT START
        public List<VendingMachineEntry> Inventory;
        public double PriceMultiplier;
        public int Credits;

        public VendingMachineInterfaceState(List<VendingMachineEntry> inventory, double priceMultiplier,
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

    [Serializable, NetSerializable]
    public sealed class VendingMachineEntityEjectMessage : BoundUserInterfaceMessage
    {
        public readonly EntityUid Uid;
        public readonly string Name;
        public VendingMachineEntityEjectMessage(EntityUid uid, string name)
        {
            Uid = uid;
            Name = name;
        }
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
