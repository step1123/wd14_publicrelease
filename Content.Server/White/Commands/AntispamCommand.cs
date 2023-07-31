using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.White;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.White.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class AntispamCommand : IConsoleCommand
{
    public string Command => "setantispam";
    public string Description => "Переключает антиспам систему.";
    public string Help => "setantispam <bool>";

    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !bool.TryParse(args[0], out var value))
        {
            shell.WriteError($"{args[0]} is not a valid boolean.");
            return;
        }

        _cfg.SetCVar(WhiteCVars.ChatAntispam, value);

        var toggle = value ? "включил" : "выключил";
        var announce = $"{shell.Player?.Name} {toggle} антиспам систему";

        IoCManager.Resolve<IChatManager>().DispatchServerAnnouncement(announce, Color.Red);
    }
}
