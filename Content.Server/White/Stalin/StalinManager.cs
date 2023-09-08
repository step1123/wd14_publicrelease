using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Shared.White;
using Content.Shared.White.SaltedYayca;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.White.Stalin;

public sealed class StalinManager
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;


    private IChatManager _chatManager = default!;

    private readonly Dictionary<string, DiscordUserData> _registeredStalinCache = new();
    private readonly Dictionary<string, DateTime> _nextStalinAllowedCheck = new();
    private string _stalinApiUrl = string.Empty;
    private string _stalinAuthUrl = string.Empty;
    private float _minimalDiscordAccountAge = 0f;

    public void Initialize()
    {
        _netManager.RegisterNetMessage<DiscordAuthRequest>(OnDiscordAuthRequest);
        _netManager.RegisterNetMessage<DiscordAuthResponse>();
        _chatManager = IoCManager.Resolve<IChatManager>();

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
        _configurationManager.OnValueChanged(WhiteCVars.StalinApiUrl, newValue => _stalinApiUrl = newValue, true);
        _configurationManager.OnValueChanged(WhiteCVars.StalinAuthUrl, newValue => _stalinAuthUrl = newValue, true);
        _configurationManager.OnValueChanged(WhiteCVars.StalinDiscordMinimumAge, newValue => _minimalDiscordAccountAge = newValue, true);
    }

    public async Task RefreshUsersData()
    {
        var players = Filter.GetAllPlayers().Cast<IPlayerSession>().ToList();

        var usersData = await RequestDiscordUsersDataAsync(players);

        if(usersData == null) return;

        foreach (var data in usersData.Users)
        {
            if(!data.Value.Registered) continue;
            _registeredStalinCache[data.Key] = data.Value;
        }
    }

    public async Task<(bool allow, string errorMessage)> AllowEnter(IPlayerSession session, bool requestIfNull = true)
    {
        var userId = session.UserId.ToString();
        if (_nextStalinAllowedCheck.TryGetValue(userId, out var nextAllowedCheckTime))
        {
            if (DateTime.Now < nextAllowedCheckTime)
            {
                var timeoutTime = (int) ((nextAllowedCheckTime - DateTime.Now).TotalSeconds);
                return (false, Loc.GetString("stalin-timeout", ("timeoutTime", timeoutTime)));
            }
        }

        var nextCheckTime = DateTime.Now.AddSeconds(_random.NextDouble(3,8));
        _nextStalinAllowedCheck[userId] = nextCheckTime;

        DiscordUserData responseData = null!;
        if (!_registeredStalinCache.TryGetValue(userId, out responseData!) && requestIfNull)
        {
            responseData = await RequestDiscordUserDataAsync(session);
        }

        if (responseData == null)
        {
            return (false, Loc.GetString("stalin-discord-doesnt-link"));
        }

        var discordAge = GetDiscordAccountAge(responseData);
        var discordAgeCheck = VerifyDiscordAge(discordAge);

        return (discordAgeCheck.passed, discordAgeCheck.errorMessage);
    }

    private (bool passed, string errorMessage) VerifyDiscordAge(double discordAge)
    {
        if(discordAge < _minimalDiscordAccountAge)
        {
            long needed = (long)(_minimalDiscordAccountAge - discordAge);
            return (false, Loc.GetString("stalin-discord-age-check-fail", ("needed", needed)));
        }

        return (true, string.Empty);
    }

    private double GetDiscordAccountAge(DiscordUserData data)
    {
        return (DateTime.Now - data.DiscordAge).TotalSeconds;
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if(!_cfg.GetCVar(WhiteCVars.StalinEnabled)) return;

        if (e.NewStatus != SessionStatus.Connected) return;

        var session = e.Session;

        if(string.IsNullOrEmpty(_stalinApiUrl))
        {
            var sawmill = Logger.GetSawmill("stalin");
            sawmill.Log(LogLevel.Warning, "Stalin API URL is not set, skipping check.");
            return;
        }

        var discordUserData = await RequestDiscordUserDataAsync(session);

        if (discordUserData == null)
        {
            return;
        }

        _registeredStalinCache[session.UserId.ToString()] = discordUserData;
    }

    /// <summary>
    /// Запрашивает данные о привязки аккаунта к дискорду. Если аккаунт не привязан, возвращает null.
    /// </summary>
    /// <param name="session"></param>
    /// <exception cref="NullReferenceException"></exception>
    /// <returns></returns>
    private async Task<DiscordUserData> RequestDiscordUserDataAsync(IPlayerSession session)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(5);
        HttpResponseMessage response;

        try
        {
            response = await client.GetAsync($"{_stalinApiUrl}isconnected?uuid={session.UserId}");
        }
        catch (Exception e)
        {
            _taskManager.RunOnMainThread(() =>
            {
                _chatManager.DispatchServerMessage(session, Loc.GetString("stalin-request-failed",
                    ("error", e.InnerException!.ToString())));

                var sawmill = Logger.GetSawmill("yayca");
                sawmill.Log(LogLevel.Warning, $"API отвалился, звоните Утке...");
            });

            return null!;
        }

        if (!response.IsSuccessStatusCode)
        {
            _taskManager.RunOnMainThread(() =>
            {
                _chatManager.DispatchServerMessage(session,
                    Loc.GetString("stalin-request-failed", ("error", response.StatusCode)));
            });

            return null!;
        }

        var result = await response.Content.ReadFromJsonAsync<DiscordUserData>();

        if (result!.Registered == false)
        {
            return null!;
        }
        return result!;
    }

    private async Task<DiscordUsersData> RequestDiscordUsersDataAsync(List<IPlayerSession> sessions)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(5);
        HttpResponseMessage response;

        try
        {
            var request = new DiscordUsersDataRequest()
            {
                Uids = sessions.Select(x => x.UserId.ToString()).ToList()
            };

            response = await client.PostAsJsonAsync(_stalinApiUrl + "isconnected", request);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null!;
        }


        var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, DiscordUserData>>();

        var usersData = new DiscordUsersData()
        {
            Users = responseData!
        };

        return usersData;
    }

    private void OnDiscordAuthRequest(DiscordAuthRequest message)
    {

        var playerSession = _playerManager.GetSessionByChannel(message.MsgChannel);

        var saltedYayca = GenerateDiscordAuthUri(playerSession.Name, playerSession.UserId.ToString());

        var response = new DiscordAuthResponse()
        {
            Uri = saltedYayca
        };

        _netManager.ServerSendMessage(response, message.MsgChannel);
    }

    private string GenerateDiscordAuthUri(string ckey, string uid)
    {
        using var sha1 = new SHA1Managed();

        var saltBytes = Encoding.UTF8.GetBytes(_configurationManager.GetCVar(WhiteCVars.StalinSalt));
        var ckeyBytes = Encoding.UTF8.GetBytes(ckey);
        var uidBytes = Encoding.UTF8.GetBytes(uid);

        var saltedBytes = ckeyBytes.Concat(uidBytes).Concat(saltBytes).ToArray();
        var hash = ToHexStr(sha1.ComputeHash(saltedBytes));

        var request = WebUtility.UrlEncode($"{ckey}@{uid}@{hash}");
        return $"{_stalinAuthUrl}{request}";
    }

    private static string ToHexStr(byte[] hash)
    {
        StringBuilder hex = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }
}
