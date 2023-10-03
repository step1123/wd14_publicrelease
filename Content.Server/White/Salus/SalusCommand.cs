using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.White;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.White;

[AdminCommand(AdminFlags.Admin)]
public sealed class SalusCommand : IConsoleCommand
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public string Command => "salus";
    public string Description => "Enables SALUS system (Autokick vpn users)";
    public string Help => "salus <bool> or salus for toggle on/off";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        IoCManager.InjectDependencies(this);

        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments",("lower", 0), ("upper", 1)));
            return;
        }

        var enabled = _cfg.GetCVar(WhiteCVars.AutoKickVpnUsers);

        if (args.Length == 0)
        {
            enabled = !enabled;
        }

        if (args.Length == 1 && !bool.TryParse(args[0], out enabled))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-boolean"));
            return;
        }

        _cfg.SetCVar(WhiteCVars.AutoKickVpnUsers, enabled);

        var announce = Loc.GetString("vpn-salus-status", ("enabled", $"{enabled}"));

        IoCManager.Resolve<IChatManager>().DispatchServerAnnouncement(announce, Color.Red);
    }
}
