using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind.Components;
using Content.Server.Popups;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Server.White.Administration;
using Content.Server.White.Sponsors;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Verbs;
using Content.Shared.White;
using Content.Shared.White.MeatyOre;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Server.White.MeatyOre;

public sealed class MeatyOreStoreSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRuleSystem = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SponsorsManager _sponsorsManager = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverrideSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly AntagRoleBanSystem _antagBan = default!;

    private HttpClient _httpClient = default!;
    private string _apiUrl = default!;

    private static readonly string StorePresetPrototype = "StorePresetMeatyOre";
    private static readonly string MeatyOreCurrencyPrototype = "MeatyOreCoin";

    private bool _meatyOrePanelEnabled;

    private readonly Dictionary<NetUserId, StoreComponent> _meatyOreStores = new();

    public override void Initialize()
    {
        base.Initialize();

        _httpClient = new HttpClient();

        _configurationManager.OnValueChanged(WhiteCVars.MeatyOrePanelEnabled, OnPanelEnableChanged, true);
        _configurationManager.OnValueChanged(WhiteCVars.OnlyInOhio, s => _apiUrl = s, true);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnPostRoundCleanup);
        SubscribeNetworkEvent<MeatyOreShopRequestEvent>(OnShopRequested);
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(MeatyOreVerbs);

    }

    private void MeatyOreVerbs(GetVerbsEvent<Verb> ev)
    {
        if (!EntityManager.TryGetComponent<ActorComponent>(ev.User, out var actorComponent))
            return;

        if (!_sponsorsManager.TryGetInfo(actorComponent.PlayerSession.UserId, out _))
            return;

        if (!HasComp<HumanoidAppearanceComponent>(ev.Target))
            return;

        if (!TryGetStore(actorComponent.PlayerSession, out var store))
            return;

        var verb = new Verb()
        {
            Text = $"Выдать роль.",
            ConfirmationPopup = true,
            Message = $"Цена - {MeatyOreCurrencyPrototype}:10",
            Act = () =>
            {
                TryAddRole(ev.User, ev.Target, store);
            },
            Category = VerbCategory.MeatyOre
        };

        ev.Verbs.Add(verb);

    }

    private void OnPanelEnableChanged(bool enabled)
    {
        if (!enabled)
        {
            foreach (var meatyOreStoreData in _meatyOreStores)
            {
                var session = _playerManager.GetSessionByUserId(meatyOreStoreData.Key);

                var playerEntity = session.AttachedEntity;

                if(!playerEntity.HasValue)
                    continue;

                _storeSystem.CloseUi(playerEntity.Value, meatyOreStoreData.Value);
            }
        }

        _meatyOrePanelEnabled = enabled;
    }
    private void OnShopRequested(MeatyOreShopRequestEvent msg, EntitySessionEventArgs args)
    {

        var playerSession = args.SenderSession as IPlayerSession;

        if (!_meatyOrePanelEnabled)
        {
            _chatManager.DispatchServerMessage(playerSession!, "Мясная панель отключена на данном сервере! Приятной игры!");
            return;
        }

        var playerEntity = args.SenderSession.AttachedEntity;

        if(!playerEntity.HasValue)
            return;

        if(!HasComp<HumanoidAppearanceComponent>(playerEntity.Value))
            return;

        if(!TryGetStore(playerSession!, out var storeComponent))
            return;

        _pvsOverrideSystem.AddSessionOverride(storeComponent.Owner, playerSession!);
        _storeSystem.ToggleUi(playerEntity.Value, storeComponent.Owner, storeComponent);
    }

    private bool TryGetStore(IPlayerSession session, out StoreComponent store)
    {
        store = null!;

        if (!_sponsorsManager.TryGetInfo(session.UserId, out var sponsorInfo))
            return false;
        if (_meatyOreStores.TryGetValue(session.UserId, out store!))
            return true;
        if (sponsorInfo.MeatyOreCoin == 0)
            return false;

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

        _storeSystem.InitializeFromPreset(StorePresetPrototype, shopEntity, storeComponent);
        storeComponent.Balance.Clear();

        _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>() { { MeatyOreCurrencyPrototype, balance } }, storeComponent.Owner, storeComponent);
        _meatyOreStores[userId] = storeComponent;
        _pvsOverrideSystem.AddSessionOverride(shopEntity, session);

        return storeComponent;
    }

    private async void TryAddRole(EntityUid user, EntityUid target, StoreComponent store)
    {
        if (!EntityManager.TryGetComponent<ActorComponent>(user, out var userActorComponent))
            return;

        if (!EntityManager.TryGetComponent<ActorComponent>(target, out var targetActorComponent))
            return;

        if (!TryComp<MindContainerComponent>(target, out var targetMind) || !targetMind.HasMind || targetMind.Mind.Session == null)
        {
            return;
        }


        var fake = _antagBan.HasAntagBan(userActorComponent.PlayerSession.UserId)
                   || _antagBan.HasAntagBan(targetMind.Mind.Session.UserId)
                   || targetMind.Mind.AllRoles.Any(x => x.Antagonist)
                   || targetMind.Mind.CurrentJob?.CanBeAntag != true;

        var ckey = userActorComponent.PlayerSession.Name;
        var grant = user == target;
        var result = await GrantAntagonist(ckey, !grant);

        if (result)
        {
            _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2> { { MeatyOreCurrencyPrototype, -10 } }, store.Owner, store);

            if (!fake)
            {
                _traitorRuleSystem.MakeTraitor(targetActorComponent.PlayerSession);

                var msg = $"Игрок с сикеем {ckey} выдал антажку {targetActorComponent.PlayerSession.Name}";
                _chatManager.SendAdminAnnouncement(msg);
            }
            else
            {
                var msg = $"Игрок с сикеем {ckey} попытался выдать антажку {targetActorComponent.PlayerSession.Name}. Но обосрался. Была выдана фейковая антажка.";
                _chatManager.SendAdminAnnouncement(msg);
            }

        }
        else
        {
            var timeMessage = grant
                ? $"Вы сможете выдать себе антага через: {await GetCooldownRemaining(ckey, false)}"
                : $"Вы сможете выдать антага другу через: {await GetCooldownRemaining(ckey, true)}";

            _popupSystem.PopupEntity(timeMessage, user, user);
        }
    }

    private async Task<bool> GrantAntagonist(string ckey, bool isFriend)
    {
        var result = false;

        try
        {
            var url = $"{_apiUrl}/api/Antagonist/grantUser";

            if (isFriend)
            {
                url = $"{_apiUrl}/api/Antagonist/grantFriend";
            }

            var requestData = new { UserId = ckey };

            var response = await _httpClient.PostAsJsonAsync(url, requestData);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                result = bool.Parse(responseContent);
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return result;
    }

    private async Task<TimeSpan> GetCooldownRemaining(string ckey, bool isFriend)
    {
        try
        {
            var url = $"{_apiUrl}/api/Antagonist/cooldownUser?userId={ckey}";

            if (isFriend)
            {
                url = $"{_apiUrl}/api/Antagonist/cooldownFriend?userId={ckey}";
            }

            HttpResponseMessage response;

            response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseData))
                {
                    var jsonDocument = JsonDocument.Parse(responseData);
                    var root = jsonDocument.RootElement;
                    if (root.TryGetProperty("remainingTime", out var remainingTimeElement) && TimeSpan.TryParse(remainingTimeElement.ToString(), out var remainingTime))
                    {
                        var time = new TimeSpan(remainingTime.Hours, remainingTime.Minutes, 0);
                        return time;
                    }
                }
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return TimeSpan.Zero;
    }
}
