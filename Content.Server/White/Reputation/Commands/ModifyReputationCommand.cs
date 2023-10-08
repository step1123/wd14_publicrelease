using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.White.Reputation.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class ModifyReputationCommand : IConsoleCommand
{
    public string Command => "modifyreput";
    public string Description => "Add the value to user's reputation.";
    public string Help => "Usage: modifyreput {ckey} {valueToAdd}";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var playerManager = IoCManager.Resolve<IPlayerManager>();
        var repManager = IoCManager.Resolve<ReputationManager>();

        if (args.Length < 2)
        {
            shell.WriteLine($"Not enough arguments.\n{Help}");
            return;
        }

        if (!playerManager.TryGetPlayerDataByUsername(args[0], out var playerData))
        {
            shell.WriteLine($"Couldn't find player: {args[0]}.");
            return;
        }

        if (!float.TryParse(args[1], out var value))
        {
            shell.WriteLine($"Invalid value: {args[1]}.");
            return;
        }

        var uid = playerData.UserId;

        repManager.ModifyPlayerReputation(uid, value);

        shell.WriteLine($"Added {args[1]} to the reputation of {args[0]}.");
    }
}
