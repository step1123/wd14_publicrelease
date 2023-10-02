using Content.Shared.Access.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.White.Economy;
using Robust.Server.GameObjects;

namespace Content.Server.White.Economy;

public sealed class EftposSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EftposComponent, EftposLockMessage>(OnLock);
        SubscribeLocalEvent<EftposComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, EftposComponent component, InteractUsingEvent args)
    {
        if (component.BankAccountId == null || !TryComp(args.Used, out BankCardComponent? bankCard) ||
            bankCard.BankAccountId == component.BankAccountId || component.Amount <= 0)
            return;

        if (_bankCardSystem.TryChangeBalance(bankCard.BankAccountId!.Value, -component.Amount) &&
            _bankCardSystem.TryChangeBalance(component.BankAccountId.Value, component.Amount))
        {
            _popupSystem.PopupEntity(Loc.GetString("eftpos-transaction-success"), uid);
            _audioSystem.PlayPvs(component.SoundApply, uid);
        }
        else
        {
            _popupSystem.PopupEntity(Loc.GetString("eftpos-transaction-error"), uid);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
        }
    }

    private void OnLock(EntityUid uid, EftposComponent component, EftposLockMessage args)
    {
        if (!TryComp(args.Session.AttachedEntity, out HandsComponent? hands) ||
            !TryComp(hands.ActiveHandEntity, out BankCardComponent? bankCard))
            return;

        if (component.BankAccountId == null)
        {
            component.BankAccountId = bankCard.BankAccountId;
            component.Amount = args.Amount;
        }
        else if (component.BankAccountId == bankCard.BankAccountId)
        {
            component.BankAccountId = null;
            component.Amount = 0;
        }

        UpdateUiState(uid, component.BankAccountId != null, component.Amount,
            GetOwner(hands.ActiveHandEntity.Value, component.BankAccountId));
    }

    private string GetOwner(EntityUid uid, int? bankAccountId)
    {
        if (bankAccountId == null || !_bankCardSystem.TryGetAccount(bankAccountId.Value, out var account))
            return string.Empty;

        if (TryComp(uid, out IdCardComponent? idCard) && idCard.FullName != null)
            return idCard.FullName;

        return account.Name == string.Empty ? account.AccountId.ToString() : account.Name;
    }

    private void UpdateUiState(EntityUid uid, bool locked, int amount, string owner)
    {
        var state = new EftposBuiState
        {
            Locked = locked,
            Amount = amount,
            Owner = owner
        };

        _ui.TrySetUiState(uid, EftposKey.Key, state);
    }
}
