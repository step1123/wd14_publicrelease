using System.Linq;
using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.White.Economy;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.White.Economy;

public sealed class ATMSystem : SharedATMSystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ATMComponent, EntInsertedIntoContainerMessage>(OnCardInserted);
        SubscribeLocalEvent<ATMComponent, EntRemovedFromContainerMessage>(OnCardRemoved);
        SubscribeLocalEvent<ATMComponent, ATMRequestWithdrawMessage>(OnWithdrawRequest);
        SubscribeLocalEvent<ATMComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ATMComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, ATMComponent component, ComponentStartup args)
    {
        UpdateUiState(uid, -1, false, Loc.GetString("atm-ui-insert-card"));
    }

    private void OnInteractUsing(EntityUid uid, ATMComponent component, InteractUsingEvent args)
    {
        if (!TryComp<CurrencyComponent>(args.Used, out var currency) || !currency.Price.Keys.Contains(component.CurrencyType))
        {
            return;
        }

        if (!component.CardSlot.Item.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-trying-insert-cash-error"), args.Target, args.User, PopupType.Medium);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        var stack = Comp<StackComponent>(args.Used);
        var bankCard = Comp<BankCardComponent>(component.CardSlot.Item.Value);
        var amount = stack.Count;

        _bankCardSystem.TryChangeBalance(bankCard.BankAccountId!.Value, amount);
        Del(args.Used);

        _audioSystem.PlayPvs(component.SoundInsertCurrency, uid);
        UpdateUiState(uid, _bankCardSystem.GetBalance(bankCard.BankAccountId.Value), true, Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void OnCardInserted(EntityUid uid, ATMComponent component, EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<BankCardComponent>(args.Entity, out var bankCard) || !bankCard.BankAccountId.HasValue)
        {
            args.Container.EmptyContainer();
            return;
        }

        UpdateUiState(uid, _bankCardSystem.GetBalance(bankCard.BankAccountId.Value), true, Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void OnCardRemoved(EntityUid uid, ATMComponent component, EntRemovedFromContainerMessage args)
    {
        UpdateUiState(uid, -1, false, Loc.GetString("atm-ui-insert-card"));
    }

    private void OnWithdrawRequest(EntityUid uid, ATMComponent component, ATMRequestWithdrawMessage args)
    {
        if (!TryComp<BankCardComponent>(component.CardSlot.Item, out var bankCard) || !bankCard.BankAccountId.HasValue)
        {
            component.CardSlot.ContainerSlot?.EmptyContainer();
            return;
        }

        if (!_bankCardSystem.TryGetAccount(bankCard.BankAccountId.Value, out var account) || account.AccountPin != args.Pin)
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-wrong-pin"), uid);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        if (!_bankCardSystem.TryChangeBalance(account.AccountId, -args.Amount))
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-not-enough-cash"), uid);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        _stackSystem.Spawn(args.Amount, _prototypeManager.Index<StackPrototype>(component.CreditStackPrototype), Transform(uid).Coordinates);
        _audioSystem.PlayPvs(component.SoundWithdrawCurrency, uid);

        UpdateUiState(uid, account.Balance, true, Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void UpdateUiState(EntityUid uid, int balance, bool hasCard, string infoMessage)
    {
        var state = new ATMBuiState
        {
            AccountBalance = balance,
            HasCard = hasCard,
            InfoMessage = infoMessage
        };


        _ui.TrySetUiState(uid, ATMUiKey.Key, state);
    }
}
