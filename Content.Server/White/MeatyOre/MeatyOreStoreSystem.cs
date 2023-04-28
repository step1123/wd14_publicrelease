using System.Linq;
using Content.Server.Actions;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind.Components;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Server.White.Sponsors;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nuke;
using Content.Shared.Verbs;
using Content.Shared.White;
using Content.Shared.White.MeatyOre;
using Content.Shared.White.Sponsors;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Players;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.White;

public sealed class MeatyOreStoreSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRuleSystem = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SponsorsManager _sponsorsManager = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly PVSOverrideSystem _pvsOverrideSystem = default!;


    private static readonly string StorePresetPrototype = "StorePresetMeatyOre";
    private static readonly string MeatyOreCurrensyPrototype = "MeatyOreCoin";
    private static bool MeatyOrePanelEnabled;


    private readonly Dictionary<NetUserId, StoreComponent> _meatyOreStores = new();
    public override void Initialize()
    {
        base.Initialize();

        _configurationManager.OnValueChanged(WhiteCVars.MeatyOrePanelEnabled, OnPanelEnableChanged, true);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnPostRoundCleanup);
        SubscribeNetworkEvent<MeatyOreShopRequestEvent>(OnShopRequested);
        SubscribeLocalEvent<MindComponent, MeatyTraitorRequestActionEvent>(OnAntagPurchase);
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(MeatyOreVerbs);

    }

    private void MeatyOreVerbs(GetVerbsEvent<Verb> ev)
    {
        if(ev.User == ev.Target) return;
        if(!EntityManager.TryGetComponent<ActorComponent>(ev.User, out var actorComponent)) return;
        if(!_sponsorsManager.TryGetInfo(actorComponent.PlayerSession.UserId, out _)) return;
        if(!HasComp<HumanoidAppearanceComponent>(ev.Target)) return;
        if(!TryComp<MobStateComponent>(ev.Target, out var state) || state?.CurrentState != MobState.Alive) return;
        if(!TryGetStore(actorComponent.PlayerSession, out var store)) return;

        if(!TryComp<MindComponent>(ev.Target, out var targetMind) || !targetMind.HasMind) return;
        if (targetMind!.Mind!.AllRoles.Any(x => x.Antagonist)) return;

        if(targetMind.Mind.CurrentJob?.CanBeAntag != true) return;
        if(targetMind.Mind.Session == null) return;

        if (!store.Balance.TryGetValue("MeatyOreCoin", out var currency)) return;

        if(currency - 10 < 0) return;

        var verb = new Verb()
        {
            Text = $"Выдать роль.",
            ConfirmationPopup = true,
            Message = $"Цена - {MeatyOreCurrensyPrototype}:10",
            Act = () =>
            {
                _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2> {{MeatyOreCurrensyPrototype, -10}}, store.Owner, store);
                _traitorRuleSystem.MakeTraitor(targetMind.Mind.Session);
            },
            Category = VerbCategory.MeatyOre
        };

        ev.Verbs.Add(verb);

    }

    private void OnPanelEnableChanged(bool newValue)
    {
        if (newValue != true)
        {
            foreach (var meatyOreStoreData in _meatyOreStores)
            {
                var session = _playerManager.GetSessionByUserId(meatyOreStoreData.Key);
                if(session == null) continue;
                var playerEntity = session.AttachedEntity;
                if(!playerEntity.HasValue) continue;

                _storeSystem.CloseUi(playerEntity.Value, meatyOreStoreData.Value);
            }
        }
        MeatyOrePanelEnabled = newValue;
    }


    private void OnAntagPurchase(EntityUid uid, MindComponent component, MeatyTraitorRequestActionEvent args)
    {
        if(component.Mind == null) return;
        if(component.Mind.Session == null) return;

        _traitorRuleSystem.MakeTraitor(component.Mind?.Session!);
    }

    private void OnShopRequested(MeatyOreShopRequestEvent msg, EntitySessionEventArgs args)
    {

        var playerSession = args.SenderSession as IPlayerSession;

        if (!MeatyOrePanelEnabled)
        {
            _chatManager.DispatchServerMessage(playerSession!, "Мясная панель отключена на данном сервере! Приятной игры!");
            return;
        }

        var playerEntity = args.SenderSession.AttachedEntity;

        if(!playerEntity.HasValue) return;
        if(!HasComp<HumanoidAppearanceComponent>(playerEntity.Value)) return;
        if(!TryGetStore(playerSession!, out var storeComponent)) return;

        _pvsOverrideSystem.AddSessionOverride(storeComponent.Owner, playerSession!);
        _storeSystem.ToggleUi(playerEntity.Value, storeComponent.Owner, storeComponent);
    }

    private bool TryGetStore(IPlayerSession session, out StoreComponent store)
    {
        store = null!;

        if (!_sponsorsManager.TryGetInfo(session.UserId, out var sponsorInfo))
        {
            return false;
        }

        if (_meatyOreStores.TryGetValue(session.UserId, out store!)) return true;
        if (sponsorInfo.MeatyOreCoin == 0) return false;

        store = CreateStore(session.UserId, sponsorInfo.MeatyOreCoin);
        return true;
    }

    private void OnPostRoundCleanup(RoundRestartCleanupEvent ev)
    {
        foreach (var store in _meatyOreStores.Values)
        {
            Del(store.Owner);
        }

        _meatyOreStores.Clear();
    }

    private StoreComponent CreateStore(NetUserId userId, int balance)
    {
        var session = _playerManager.GetSessionByUserId(userId);
        var shopEntity = _entityManager.SpawnEntity("StoreMeatyOreEntity", MapCoordinates.Nullspace);
        var storeComponent = Comp<StoreComponent>(shopEntity);

        _storeSystem.InitializeFromPreset("StorePresetMeatyOre", shopEntity, storeComponent);
        storeComponent.Balance.Clear();

        _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>() { { MeatyOreCurrensyPrototype, balance } }, storeComponent.Owner, storeComponent);
        _meatyOreStores[userId] = storeComponent;
        _pvsOverrideSystem.AddSessionOverride(shopEntity, session);

        return storeComponent;
    }




}
