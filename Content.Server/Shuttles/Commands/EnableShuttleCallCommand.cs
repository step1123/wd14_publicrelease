using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.White;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.Shuttles.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class EnableShuttleCallCommand : IConsoleCommand
{
    public string Command => "enableShuttleCall";
    public string Description => Loc.GetString("Toggles the shuttle call.");
    public string Help => $"{Command} <bool>";

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !bool.TryParse(args[0], out bool value))
        {
            shell.WriteError($"{args[0]} is not a valid boolean.");
            return;
        }

        var shuttleEnabled = _cfg.GetCVar(WhiteCVars.EmergencyShuttleCallEnabled);
        if (value == shuttleEnabled)
        {
            shell.WriteError($"enableShuttleCall is already {args[0]}");
            return;
        }

        _cfg.SetCVar(WhiteCVars.EmergencyShuttleCallEnabled, value);

        var announce = Loc.GetString("emergency_shuttle-announce-toggle",
            ("admin", $"{shell.Player?.Name}"),
            ("value", $"{value}"));
        IoCManager.Resolve<IChatManager>().SendAdminAnnouncement(announce);
    }
}
