using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.White;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.White.Stalin.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class EnableStalinBunker : IConsoleCommand
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public string Command => "stalinbunker";
    public string Description => "Enables the stalin bunker, like PaNIk bunker, but better";
    public string Help => "stalinBunker <bool>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments",("lower", 0), ("upper", 1)));
            return;
        }

        var enabled = _cfg.GetCVar(CCVars.PanicBunkerEnabled);

        if (args.Length == 0)
        {
            enabled = !enabled;
        }

        if (args.Length == 1 && !bool.TryParse(args[0], out enabled))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-boolean"));
            return;
        }

        IoCManager.InjectDependencies(this);

        _cfg.SetCVar(WhiteCVars.StalinEnabled, enabled);

        var announce = Loc.GetString("stalin-panic-bunker", ("enabled", $"{enabled}"));

        IoCManager.Resolve<IChatManager>().DispatchServerAnnouncement(announce, Color.Red);
    }
}
