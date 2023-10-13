using System.Text;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Shared.Console;

namespace Content.Server.White.JobWhitelist;

[AdminCommand(AdminFlags.JobWhitelist)]
public sealed class ListJobWhitelistCommand : IConsoleCommand
{
    [Dependency] private readonly JobWhitelistManager _manager = default!;
    [Dependency] private readonly IPlayerLocator _locator = default!;
    public string Command => "jobwhitelistlist";
    public string Description => Loc.GetString("cmd-jobwhitelistlist-description");
    public string Help => Loc.GetString("cmd-jobwhitelistlist-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistlist-arg-count"));
            shell.WriteLine(Help);
            return;
        }

        var includeUnbanned = true;

        var located = await _locator.LookupIdByNameOrIdAsync(args[0]);
        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistlist-player"));
            return;
        }

        var wls = _manager.TakeAllowedJobs(located.UserId);

        if (wls.Count == 0)
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistlist-no-player"));
            return;
        }

        var wlString = new StringBuilder("WhitelistedJobs in record:\n");

        wlString
            .Append("User ID: ")
            .Append(located.Username)
            .Append('\n');

        foreach (var wl in wls)
        {
            wlString
                .Append("Job: ")
                .Append(wl)
                .Append('\n');
        }

        shell.WriteLine(wlString.ToString());
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-jobwhitelistlist-hint-1")),
            _ => CompletionResult.Empty
        };
    }
}
