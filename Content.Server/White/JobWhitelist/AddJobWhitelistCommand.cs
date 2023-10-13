using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.White.JobWhitelist;

[AdminCommand(AdminFlags.JobWhitelist)]
public sealed class AddJobWhitelistCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly JobWhitelistManager _manager = default!;
    private ISawmill _sawmill = default!;

    public string Command => "jobwhitelistadd";
    public string Description => Loc.GetString("cmd-jobwhitelistadd-description");
    public string Help => Loc.GetString("cmd-jobwhitelistadd-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistadd-arg-count"));
            shell.WriteLine(Help);
            return;
        }

        var located = await _locator.LookupIdByNameOrIdAsync(args[0]);
        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistadd-player"));
            return;
        }

        if (!_prototype.TryIndex<JobPrototype>(args[1], out var job))
        {
            shell.WriteError(Loc.GetString("cmd-jobwhitelistadd-job"));
            return;
        }

        _sawmill = Logger.GetSawmill("jobwhitelist");

        if (shell.IsClient)
            _sawmill.Info($"{shell.Player} add {job.ID} to jobwhitelist for {located.Username}");
        else
            _sawmill.Info($"Server add {job.ID} to jobwhitelist for {located.Username}");

        _manager.AddToWhitelist(located.UserId, job.ID);
        shell.WriteLine(Loc.GetString("cmd-jobwhitelistadd-success"));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-jobwhitelistadd-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<JobPrototype>(),
                Loc.GetString("cmd-jobwhitelistadd-hint-2")),
            _ => CompletionResult.Empty
        };
    }
}
