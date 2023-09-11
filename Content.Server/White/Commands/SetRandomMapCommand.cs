using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.White.Commands;

[AdminCommand(AdminFlags.Round)]
sealed class SetRanomMapCommand : IConsoleCommand
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    public string Command => "setrandommap";
    public string Description => "Устанавливает случайную карту из пула и включает ротацию карт";
    public string Help => "setrandommap";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _configurationManager.SetCVar(CCVars.GameMap, string.Empty);
        shell.WriteLine("Включена ротация карт.");
    }
}
