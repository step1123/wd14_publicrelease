using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Content.Shared.White.Economy;

namespace Content.Server.White.Economy;

public sealed class BankCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BankCartridgeComponent, CartridgeMessageEvent>(OnUiMessage);
        SubscribeLocalEvent<BankCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
    }

    private void OnUiReady(EntityUid uid, BankCartridgeComponent component, CartridgeUiReadyEvent args)
    {
        UpdateUiState(uid, args.Loader, component);
    }

    private void OnUiMessage(EntityUid uid, BankCartridgeComponent component, CartridgeMessageEvent args)
    {
        UpdateUiState(uid, args.LoaderUid, component);
    }

    public void UpdateUiState(EntityUid cartridgeUid, EntityUid loaderUid, BankCartridgeComponent? component = null)
    {
        if (!Resolve(cartridgeUid, ref component))
            return;

        var state = new BankCartridgeUiState
        {
            AccountLinked = component.AccountId != null
        };

        if (component.AccountId != null && _bankCardSystem.TryGetAccount(component.AccountId.Value, out var account))
        {
            state.Balance = account.Balance;
            state.OwnerName = account.Name;
        }
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loaderUid, state);
    }
}
