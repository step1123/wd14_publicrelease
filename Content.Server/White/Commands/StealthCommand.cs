using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Utility;

namespace Content.Server.White.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class StealthCommand : IConsoleCommand
{
    public string Command => "stealth";
    public string Description => "Переключает стелс режим.";
    public string Help => "stealth";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not IPlayerSession player)
        {
            shell.WriteLine("You cannot use this command from the server console.");
            return;
        }

        var mgr = IoCManager.Resolve<IAdminManager>();
        var data = mgr.GetAdminData(player)!;
        DebugTools.AssertNotNull(data);

        data.Stealth = !data.Stealth;

        shell.WriteLine(data.Stealth
            ? "Теперь вы в режиме стелс"
            : "Теперь вы не в режиме стелс");
    }
}
