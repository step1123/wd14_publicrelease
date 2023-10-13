using Robust.Shared.GameStates;

namespace Content.Shared.White.Economy;

[RegisterComponent, NetworkedComponent]
public sealed class BankCardComponent : Component
{
    [DataField("accountId")] public readonly int? AccoundId;

    [ViewVariables]
    public int? BankAccountId;

    [DataField("startingBalance")]
    public int StartingBalance = 0;

    [DataField("commandBudgetCard")]
    public bool CommandBudgetCard;
}
