using Robust.Shared.Console;

namespace Content.Server.White.ERTRecruitment.Commands;

public sealed class BlockERT : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entities = default!;

    public string Command => "blockert";
    public string Description => "block or unblock call ERT";
    public string Help => "blockert <true/false>";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var ertsys = _entities.System<ERTRecruitmentSystem>();
        var isBlocked = !ertsys.IsBlocked;

        if (args.Length > 0)
        {
            isBlocked = args[0] == "true";
        }

        ertsys.IsBlocked = isBlocked;
        shell.WriteLine(isBlocked ? "ERT is blocked!" : "ERT is no longer blocked!");
    }
}
