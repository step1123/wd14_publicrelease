namespace Content.Server.White.Economy;

public sealed class BankAccount
{
    public readonly int AccountId;
    public readonly int AccountPin;
    public int Balance;
    public bool CommandBudgetAccount;
    public Mind.Mind? Mind;
    public string Name = string.Empty;

    public EntityUid? CartridgeUid;
    public EntityUid? LoaderUid;

    public BankAccount(int accountId, int balance)
    {
        AccountId = accountId;
        Balance = balance;
        AccountPin = Random.Shared.Next(1000, 10000);
    }
}

