using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared.Players;
using Content.Server.GameTicking;
using Content.Server.UtkaIntegration;
using Content.Server.White;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.Managers;

public sealed class RoleBanManager
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly UtkaTCPWrapper _utkaSockets = default!; // WD

    private const string JobPrefix = "Job:";

    private ISawmill _sawmill = default!;

    private readonly Dictionary<NetUserId, HashSet<ServerRoleBanDef>> _cachedRoleBans = new();

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("rolebans");
        _netManager.RegisterNetMessage<MsgRoleBans>();
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus != SessionStatus.Connected
            || _cachedRoleBans.ContainsKey(e.Session.UserId))
        {
            return;
        }

        var netChannel = e.Session.ConnectedClient;
        await CacheDbRoleBans(e.Session.UserId, netChannel.RemoteEndPoint.Address, netChannel.UserData.HWId.Length == 0 ? null : netChannel.UserData.HWId);
        SendRoleBans(e.Session);
    }

    private async Task<bool> AddRoleBan(ServerRoleBanDef banDef)
    {
        if (banDef.UserId != null)
        {
            if (!_cachedRoleBans.TryGetValue(banDef.UserId.Value, out var roleBans))
            {
                roleBans = new HashSet<ServerRoleBanDef>();
                _cachedRoleBans.Add(banDef.UserId.Value, roleBans);
            }

            roleBans.Add(banDef);
        }

        await _db.AddServerRoleBanAsync(banDef);
        return true;
    }

    public void SendRoleBans(LocatedPlayerData located)
    {
        if (!_playerManager.TryGetSessionById(located.UserId, out var player))
        {
            return;
        }

        SendRoleBans(player);
    }

    public void SendRoleBans(IPlayerSession pSession)
    {
        if (!_cachedRoleBans.TryGetValue(pSession.UserId, out var roleBans))
        {
            _sawmill.Error($"Tried to send rolebans for {pSession.Name} but none cached?");
            return;
        }

        var bans = new MsgRoleBans()
        {
            Bans = roleBans.Select(o => o.Role).ToList()
        };

        _sawmill.Debug($"Sent rolebans to {pSession.Name}");
        _netManager.ServerSendMessage(bans, pSession.ConnectedClient);
    }

    public HashSet<string>? GetRoleBans(NetUserId playerUserId)
    {
        return _cachedRoleBans.TryGetValue(playerUserId, out var roleBans) ? roleBans.Select(banDef => banDef.Role).ToHashSet() : null;
    }

    private async Task CacheDbRoleBans(NetUserId userId, IPAddress? address = null, ImmutableArray<byte>? hwId = null)
    {
        var roleBans = await _db.GetServerRoleBansAsync(address, userId, hwId, false);

        var userRoleBans = new HashSet<ServerRoleBanDef>();
        foreach (var ban in roleBans)
        {
            userRoleBans.Add(ban);
        }

        _cachedRoleBans[userId] = userRoleBans;
    }

    public void Restart()
    {
        // Clear out players that have disconnected.
        var toRemove = new List<NetUserId>();
        foreach (var player in _cachedRoleBans.Keys)
        {
            if (!_playerManager.TryGetSessionById(player, out _))
                toRemove.Add(player);
        }

        foreach (var player in toRemove)
        {
            _cachedRoleBans.Remove(player);
        }

        // Check for expired bans
        foreach (var (_, roleBans) in _cachedRoleBans)
        {
            roleBans.RemoveWhere(ban => DateTimeOffset.Now > ban.ExpirationTime);
        }
    }

    #region Job Bans
    public async void CreateJobBan(IConsoleShell shell, LocatedPlayerData located, string job, string reason, uint minutes, bool isGlobalBan)
    {
        if (!_prototypeManager.TryIndex(job, out JobPrototype? _))
        {
            shell.WriteError(Loc.GetString("cmd-roleban-job-parse", ("job", job)));
            return;
        }

        job = string.Concat(JobPrefix, job);
        CreateRoleBan(shell, located, job, reason, minutes);
    }

    //WD start
    public async void UtkaCreateJobBan(string admin, string target, string job, string reason, uint minutes, bool isGlobalBan)
    {
        var role = string.Concat(JobPrefix, job);

        var located = await _playerLocator.LookupIdByNameOrIdAsync(target);
        if (located == null)
        {
            UtkaSendResponse(false);
            return;
        }

        var targetUid = located.UserId;
        var targetHWid = located.LastHWId;
        var targetAddress = located.LastAddress;

        DateTimeOffset? expires = null;
        if (minutes > 0)
        {
            expires = DateTimeOffset.Now + TimeSpan.FromMinutes(minutes);
        }

        (IPAddress, int)? addressRange = null;
        if (targetAddress != null)
        {
            if (targetAddress.IsIPv4MappedToIPv6)
                targetAddress = targetAddress.MapToIPv4();

            // Ban /64 for IPv4, /32 for IPv4.
            var cidr = targetAddress.AddressFamily == AddressFamily.InterNetworkV6 ? 64 : 32;
            addressRange = (targetAddress, cidr);
        }

        var cfg = UnsafePseudoIoC.ConfigurationManager;
        var serverName = cfg.GetCVar(CCVars.AdminLogsServerName);

        if (isGlobalBan)
        {
            serverName = "unknown";
        }

        var locatedPlayer = await _playerLocator.LookupIdByNameOrIdAsync(admin);
        if (locatedPlayer == null)
        {
            UtkaSendResponse(false);
            return;
        }

        var player = locatedPlayer.UserId;
        var banDef = new ServerRoleBanDef(
            null,
            targetUid,
            addressRange,
            targetHWid,
            DateTimeOffset.Now,
            expires,
            reason,
            player,
            null,
            role,
            serverName);

        if (!await AddRoleBan(banDef))
        {
            UtkaSendResponse(false);
            return;
        }

        var banId = await UtkaGetBanId(reason, role, targetUid);

        UtkaSendJobBanEvent(admin, target, minutes, job, isGlobalBan, reason, banId);
        UtkaSendResponse(true);
    }
    //WD end

    public HashSet<string>? GetJobBans(NetUserId playerUserId)
    {
        if (!_cachedRoleBans.TryGetValue(playerUserId, out var roleBans))
            return null;
        return roleBans
            .Where(ban => ban.Role.StartsWith(JobPrefix, StringComparison.Ordinal))
            .Select(ban => ban.Role[JobPrefix.Length..])
            .ToHashSet();
    }
    #endregion

    #region Commands
    private async void CreateRoleBan(IConsoleShell shell, LocatedPlayerData located, string role, string reason, uint minutes, bool isGlobalBan)
    {
        var targetUid = located.UserId;
        var targetHWid = located.LastHWId;
        var targetAddress = located.LastAddress;

        DateTimeOffset? expires = null;
        if (minutes > 0)
        {
            expires = DateTimeOffset.Now + TimeSpan.FromMinutes(minutes);
        }

        (IPAddress, int)? addressRange = null;
        if (targetAddress != null)
        {
            if (targetAddress.IsIPv4MappedToIPv6)
                targetAddress = targetAddress.MapToIPv4();

            // Ban /64 for IPv4, /32 for IPv4.
            var cidr = targetAddress.AddressFamily == AddressFamily.InterNetworkV6 ? 64 : 32;
            addressRange = (targetAddress, cidr);
        }

        var cfg = UnsafePseudoIoC.ConfigurationManager;
        var serverName = cfg.GetCVar(CCVars.AdminLogsServerName);

        if (isGlobalBan)
        {
            serverName = "unknown";
        }

        var player = shell.Player as IPlayerSession;
        var banDef = new ServerRoleBanDef(
            null,
            targetUid,
            addressRange,
            targetHWid,
            DateTimeOffset.Now,
            expires,
            reason,
            player?.UserId,
            null,
            role,
            serverName);

        if (!await AddRoleBan(banDef))
        {
            shell.WriteLine(Loc.GetString("cmd-roleban-existing", ("target", located.Username), ("role", role)));
            return;
        }

        var length = expires == null ? Loc.GetString("cmd-roleban-inf") : Loc.GetString("cmd-roleban-until", ("expires", expires));
        shell.WriteLine(Loc.GetString("cmd-roleban-success", ("target", located.Username), ("role", role), ("reason", reason), ("length", length), ("server", serverName)));

        // WD start
        var banId = await UtkaGetBanId(reason, role, targetUid);

        if (job != null)
            UtkaSendJobBanEvent(shell.Player!.Name, target, minutes, job, isGlobalBan, reason, banId);
        //WD end
    }
    #endregion

    //WD start
    private void UtkaSendResponse(bool banned)
    {
        var utkaBanned = new UtkaJobBanResponse()
        {
            Banned = banned
        };

        _utkaSockets.SendMessageToAll(utkaBanned);
    }

    private async void UtkaSendJobBanEvent(string ackey, string ckey, uint duration, string job, bool global,
        string reason, int banId)
    {
        var utkaBanned = new UtkaBannedEvent()
        {
            ACkey = ackey,
            Ckey = ckey,
            Duration = duration,
            Bantype = job,
            Global = global,
            Reason = reason,
            Rid = EntitySystem.Get<GameTicker>().RoundId,
            BanId = banId
        };

        _utkaSockets.SendMessageToAll(utkaBanned);
    }

    private async Task<int> UtkaGetBanId(string reason, string role, NetUserId targetUid)
    {
        var banId = 0;
        var banList = await _db.GetServerRoleBansAsync(null, targetUid, null);

        foreach (var ban in banList)
        {
            if (ban.Reason == reason)
            {
                if (ban.Role == role && ban.Id != null)
                {
                    banId = ban.Id.Value;
                }
            }
        }

        return banId;
    }
    //WD end
}
