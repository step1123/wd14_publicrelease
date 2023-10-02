using Content.Server.White.Economy;
using Content.Shared.Cargo;

namespace Content.Server.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track its money.
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed class StationBankAccountComponent : Component
{
    // WD EDIT START
    [ViewVariables(VVAccess.ReadWrite)]
    public int Balance
    {
        get => BankAccount.Balance;
        set => BankAccount.Balance = value;
    }

    [ViewVariables]
    public BankAccount BankAccount = default!;
    // WD EDIT END

    /// <summary>
    /// How much the bank balance goes up per second, every Delay period. Rounded down when multiplied.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("increasePerSecond")]
    public int IncreasePerSecond = 1;
}
