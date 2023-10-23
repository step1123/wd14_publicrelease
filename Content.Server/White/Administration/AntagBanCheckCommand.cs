using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.White.Administration;

[AdminCommand(AdminFlags.Admin)]
public sealed class AntagBanCheckCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerLocator _locator = default!;

    public string Command => "hasantagban";
    public string Description => "is player have antagban?";
    public string Help => "hasantagban <CKEY>";
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var located = await _locator.LookupIdByNameOrIdAsync(args[0]);

        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-roleban-name-parse"));
            return;
        }
        var entMan = IoCManager.Resolve<IEntityManager>();
        shell.WriteLine($"Is player antagbanned: {entMan.EntitySysManager.GetEntitySystem<AntagRoleBanSystem>().HasAntagBan(located.UserId)}");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "CKEY");
    }

}
