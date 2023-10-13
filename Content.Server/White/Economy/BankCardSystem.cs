using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Access.Systems;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.Station.Systems;
using Content.Shared.CartridgeLoader;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.White.Economy;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.White.Economy;

public sealed class BankCardSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly BankCartridgeSystem _bankCartridge = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private const int SalaryDelay = 1200;

    private SalaryPrototype _salaries = default!;
    private readonly List<BankAccount> _accounts = new();
    private float _salaryTimer;

    public override void Initialize()
    {
        _salaries = _protoMan.Index<SalaryPrototype>("Salaries");

        SubscribeLocalEvent<BankCardComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_gameTicker.RunLevel != GameRunLevel.InRound)
        {
            _salaryTimer = 0f;
            return;
        }

        _salaryTimer += frameTime;

        if (_salaryTimer <= SalaryDelay)
            return;

        _salaryTimer = 0f;
        PaySalary();
    }

    private void PaySalary()
    {
        foreach (var account in _accounts.Where(account =>
                     account.Mind is {UserId: not null, CurrentEntity: not null} &&
                     _playerManager.TryGetSessionById(account.Mind.UserId.Value, out _) &&
                     !_mobState.IsDead(account.Mind.CurrentEntity.Value)))
        {
            account.Balance += GetSalary(account.Mind);
        }

        _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("salary-pay-announcement"),
            colorOverride: Color.FromHex("#18abf5"));
    }

    private int GetSalary(Mind.Mind? mind)
    {
        var job = mind?.CurrentJob;
        if (job == null || !_salaries.Salaries.TryGetValue(job.Prototype.ID, out var salary))
            return 0;

        return salary;
    }

    private void OnStartup(EntityUid uid, BankCardComponent component, ComponentStartup args)
    {
        if (component.CommandBudgetCard &&
            TryComp(_station.GetOwningStation(uid), out StationBankAccountComponent? acc))
        {
            component.BankAccountId = acc.BankAccount.AccountId;
            return;
        }
        if (component.AccoundId.HasValue)
        {
            CreateAccount(component.AccoundId.Value, component.StartingBalance);
            return;
        }

        var account = CreateAccount(default, component.StartingBalance);
        component.BankAccountId = account.AccountId;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _accounts.Clear();
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (_idCardSystem.TryFindIdCard(ev.Mob, out var id) && TryComp<MindContainerComponent>(ev.Mob, out var mind))
        {
            var cardEntity = id.Owner;
            var bankCardComponent = cardEntity.EnsureComponent<BankCardComponent>();

            if (!bankCardComponent.BankAccountId.HasValue || !TryGetAccount(bankCardComponent.BankAccountId.Value, out var bankAccount))
                return;

            bankAccount.Balance = GetSalary(mind.Mind) + 100;
            mind.Mind?.AddMemory(new Memory("PIN", bankAccount.AccountPin.ToString()));
            bankAccount.Mind = mind.Mind;
            bankAccount.Name = Name(ev.Mob);

            if (!_inventorySystem.TryGetSlotEntity(ev.Mob, "id", out var pdaUid) ||
                !TryComp(pdaUid, out CartridgeLoaderComponent? cartridgeLoader))
                return;

            BankCartridgeComponent? comp = null;
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            var program = cartridgeLoader.InstalledPrograms.Find(program => TryComp(program, out comp));
            if (comp == null)
                return;

            bankAccount.CartridgeUid = program;
            bankAccount.LoaderUid = pdaUid;
            comp.AccountId = bankAccount.AccountId;
        }
    }

    public BankAccount CreateAccount(int accountId = default, int startingBalance = 0)
    {
        if (TryGetAccount(accountId, out var acc))
            return acc;

        BankAccount account;
        if (accountId == default)
        {
            int accountNumber;

            do
            {
                accountNumber = _random.Next(100000, 999999);
            } while (AccountExist(accountId));

            account = new BankAccount(accountNumber, startingBalance);
        }
        else
        {
            account = new BankAccount(accountId, startingBalance);
        }

        _accounts.Add(account);

        return account;
    }

    public bool AccountExist(int accountId)
    {
        return _accounts.Any(x => x.AccountId == accountId);
    }

    public bool TryGetAccount(int accountId, [NotNullWhen(true)] out BankAccount? account)
    {
        account = _accounts.FirstOrDefault(x => x.AccountId == accountId);
        return account != null;
    }

    public int GetBalance(int accountId)
    {
        if (TryGetAccount(accountId, out var account))
        {
            return account.Balance;
        }

        return 0;
    }

    public bool TryChangeBalance(int accountId, int amount)
    {
        if (!TryGetAccount(accountId, out var account) || account.Balance + amount < 0)
            return false;

        if (account.CommandBudgetAccount)
        {
            while (AllEntityQuery<StationBankAccountComponent>().MoveNext(out var uid, out var acc))
            {
                if (acc.BankAccount.AccountId != accountId)
                    continue;

                _cargo.UpdateBankAccount(uid, acc, amount);
                return true;
            }
        }

        account.Balance += amount;
        if (account is {CartridgeUid: not null, LoaderUid: not null})
            _bankCartridge.UpdateUiState(account.CartridgeUid.Value, account.LoaderUid.Value);

        return true;
    }
}

