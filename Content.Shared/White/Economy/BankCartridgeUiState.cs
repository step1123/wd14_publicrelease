using Robust.Shared.Serialization;

namespace Content.Shared.White.Economy;

[Serializable, NetSerializable]
public sealed class BankCartridgeUiState : BoundUserInterfaceState
{
    public int Balance;
    public string OwnerName = string.Empty;
    public bool AccountLinked;
}
