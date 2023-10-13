using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.White.JobWhitelist;

[AdminCommand(AdminFlags.JobWhitelist)]
public sealed class RemoveJobWhitelistCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly JobWhitelistManager _manager = default!;
    private ISawmill _sawmill = default!;
    public string Command => "jobwhitelistremove";
    public string Description => Loc.GetString("cmd-jobwhitelistremove-description");
    public string Help => Loc.GetString("cmd-jobwhitelistremove-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        LocatedPlayerData? located;
        string? job = null;

        switch (args.Length)
        {
            case 1:
                located = await _locator.LookupIdByNameOrIdAsync(args[0]);
                break;
            case 2:
                located = await _locator.LookupIdByNameOrIdAsync(args[0]);
                job = args[1];
                break;
            default:
                shell.WriteError(Loc.GetString("cmd-jobwhitelistremove-arg-count"));
                shell.WriteLine(Help);
                return;
        }

        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistremove-player"));
            return;
        }

        if (job != null && !_prototype.TryIndex<JobPrototype>(job, out _))
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistremove-job"));
            return;
        }

        var player = located.UserId;

        _sawmill = Logger.GetSawmill("jobwhitelist");

        if (shell.IsClient)
            _sawmill.Info($"{shell.Player} remove {job} from jobwhitelist for {located.Username}");
        else
            _sawmill.Info($"Server remove {job} from jobwhitelist for {located.Username}");

        _manager.RemoveFromWhitelist(player, job);
        shell.WriteLine(Loc.GetString("cmd-jobwhitelistremove-success"));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-jobwhitelistremove-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<JobPrototype>(),
                Loc.GetString("cmd-jobwhitelistremove-hint-2")),
            _ => CompletionResult.Empty
        };
    }
}
