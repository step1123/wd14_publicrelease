using System.Net;
using System.Net.Sockets;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaBanCommand : IUtkaCommand
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private UtkaTCPWrapper _utkaSocketWrapper = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    private const ILocalizationManager LocalizationManager = default!;

    public string Name => "ban";
    public Type RequestMessageType => typeof(UtkaBanRequest);
    public async void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if (baseMessage is not UtkaBanRequest message) return;

        var plyMgr = IoCManager.Resolve<IPlayerManager>();
        var locator = IoCManager.Resolve<IPlayerLocator>();
        var dbMan = IoCManager.Resolve<IServerDbManager>();
        IoCManager.InjectDependencies(this);

        var locatedPlayer = await locator.LookupIdByNameOrIdAsync(message.ACkey!);
        if (locatedPlayer == null)
        {
            UtkaSendResponse(false);
            return;
        }

        var player = locatedPlayer.UserId;

        var target = message.Ckey!;
        var reason = message.Reason!;
        var minutes = (uint) message.Duration!;
        var isGlobalBan = (bool) message.Global!;

        var located = await locator.LookupIdByNameOrIdAsync(target);
        if (located == null)
        {
            UtkaSendResponse(false);
            return;
        }

        var targetUid = located.UserId;
        var targetHWid = located.LastHWId;
        var targetAddr = located.LastAddress;

        if (player == targetUid)
        {
            UtkaSendResponse(false);
            return;
        }

        DateTimeOffset? expires = null;
        if (minutes > 0)
        {
            expires = DateTimeOffset.Now + TimeSpan.FromMinutes(minutes);
        }

        (IPAddress, int)? addrRange = null;
        if (targetAddr != null)
        {
            if (targetAddr.IsIPv4MappedToIPv6)
                targetAddr = targetAddr.MapToIPv4();

            // Ban /64 for IPv4, /32 for IPv4.
            var cidr = targetAddr.AddressFamily == AddressFamily.InterNetworkV6 ? 64 : 32;
            addrRange = (targetAddr, cidr);
        }

        var serverName = _cfg.GetCVar(CCVars.AdminLogsServerName);

        if (isGlobalBan)
        {
            serverName = "unknown";
        }

        var banDef = new ServerBanDef(
            null,
            targetUid,
            addrRange,
            targetHWid,
            DateTimeOffset.Now,
            expires,
            reason,
            player,
            null,
            serverName);

        UtkaSendResponse(true);

        await dbMan.AddServerBanAsync(banDef);

        if (plyMgr.TryGetSessionById(targetUid, out var targetPlayer))
        {
            var msg = banDef.FormatBanMessage(_cfg, LocalizationManager);
            targetPlayer.ConnectedClient.Disconnect(msg);
        }

        var banlist = await _db.GetServerBansAsync(null, targetUid, null);
        var banId = banlist[^1].Id;

        var utkaBanned = new UtkaBannedEvent()
        {
            Ckey = message.Ckey,
            ACkey = message.ACkey,
            Bantype = "server",
            Duration = message.Duration,
            Global = message.Global,
            Reason = message.Reason,
            Rid = EntitySystem.Get<GameTicker>().RoundId,
            BanId = banId
        };
        _utkaSocketWrapper.SendMessageToAll(utkaBanned);
        _entMan.EventBus.RaiseEvent(EventSource.Local, utkaBanned);
    }

    private void UtkaSendResponse(bool banned)
    {
        var utkaResponse = new UtkaBanResponse()
        {
            Banned = banned
        };

        _utkaSocketWrapper.SendMessageToAll(utkaResponse);
    }
}
