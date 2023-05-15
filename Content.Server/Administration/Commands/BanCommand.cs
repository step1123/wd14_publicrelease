using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.UtkaIntegration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;


namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Ban)]
    public sealed class BanCommand : LocalizedCommands
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly UtkaTCPWrapper _utkaSockets = default!; // WD

        public override string Command => "ban";

        public override async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player as IPlayerSession;
            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            var locator = IoCManager.Resolve<IPlayerLocator>();
            var dbMan = IoCManager.Resolve<IServerDbManager>();

            string target;
            string reason;
            uint minutes;
            bool isGlobalBan;

            switch (args.Length)
            {
                case 2:
                    target = args[0];
                    reason = args[1];
                    minutes = 0;
                    isGlobalBan = false;
                    break;
                case 3:
                    target = args[0];
                    reason = args[1];
                    isGlobalBan = false;

                    if (!uint.TryParse(args[2], out minutes))
                    {
                        shell.WriteLine($"{args[2]} is not a valid amount of minutes.\n{Help}");
                        return;
                    }

                    break;
                case 4:
                    target = args[0];
                    reason = args[1];
                    var possibleMinutes = args[2] != "" ? args[2] : "0";

                    if (!uint.TryParse(possibleMinutes, out minutes))
                    {
                        shell.WriteLine($"{args[2]} is not a valid amount of minutes.\n{Help}");
                        return;
                    }

                    if (!bool.TryParse(args[3], out isGlobalBan))
                    {
                        shell.WriteLine($"{args[3]} should be True or False.\n{Help}");
                        return;
                    }
                    break;
                default:
                    shell.WriteLine($"Invalid amount of arguments.{Help}");
                    return;
            }

            var located = await locator.LookupIdByNameOrIdAsync(target);
            if (located == null)
            {
                shell.WriteError(LocalizationManager.GetString("cmd-ban-player"));
                return;
            }

            var targetUid = located.UserId;
            var targetHWid = located.LastHWId;
            var targetAddr = located.LastAddress;

            if (player != null && player.UserId == targetUid)
            {
                shell.WriteLine(LocalizationManager.GetString("cmd-ban-self"));
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
                player?.UserId,
                null,
                serverName);

            await dbMan.AddServerBanAsync(banDef);

            var serverNameYaica = serverName == "unknown" ? "всех серверах" : $"сервере {serverName}";

            var response = new StringBuilder($"Забанен {target} с причиной \"{reason}\", на {serverNameYaica},");

            response.Append(expires == null ? " навсегда." : $" до {expires}");

            shell.WriteLine(response.ToString());

            if (plyMgr.TryGetSessionById(targetUid, out var targetPlayer))
            {
                var message = banDef.FormatBanMessage(_cfg, LocalizationManager);
                targetPlayer.ConnectedClient.Disconnect(message);
            }

            //WD start
            var banlist = await dbMan.GetServerBansAsync(null, targetUid, null);
            var banId = banlist[^1].Id;

            var utkaBanned = new UtkaBannedEvent()
            {
                Ckey = target,
                ACkey = player?.Name,
                Bantype = "server",
                Duration = minutes,
                Global = isGlobalBan,
                Reason = reason,
                Rid = EntitySystem.Get<GameTicker>().RoundId,
                BanId = banId
            };
            _utkaSockets.SendMessageToAll(utkaBanned);
            //WD end
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var playerMgr = IoCManager.Resolve<IPlayerManager>();
                var options = playerMgr.ServerSessions.Select(c => c.Name).OrderBy(c => c).ToArray();
                return CompletionResult.FromHintOptions(options, LocalizationManager.GetString("cmd-ban-hint"));
            }

            if (args.Length == 2)
                return CompletionResult.FromHint(LocalizationManager.GetString("cmd-ban-hint-reason"));

            if (args.Length == 3)
            {
                var durations = new CompletionOption[]
                {
                    new("0", LocalizationManager.GetString("cmd-ban-hint-duration-1")),
                    new("1440", LocalizationManager.GetString("cmd-ban-hint-duration-2")),
                    new("4320", LocalizationManager.GetString("cmd-ban-hint-duration-3")),
                    new("10080", LocalizationManager.GetString("cmd-ban-hint-duration-4")),
                    new("20160", LocalizationManager.GetString("cmd-ban-hint-duration-5")),
                    new("43800", LocalizationManager.GetString("cmd-ban-hint-duration-6")),
                };

                return CompletionResult.FromHintOptions(durations, LocalizationManager.GetString("cmd-ban-hint-duration"));
            }

            return CompletionResult.Empty;
        }
    }
}
