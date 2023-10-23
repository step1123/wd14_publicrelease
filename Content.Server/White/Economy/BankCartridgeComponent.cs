namespace Content.Server.White.Economy;

[RegisterComponent]
public sealed class BankCartridgeComponent : Component
{
    [ViewVariables]
    public int? AccountId;

    [ViewVariables]
    public EntityUid? Loader;

    public string AccountLinkResult = string.Empty;
}
