using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.White.ERTRecruitment.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class BlockERT : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

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
        var message = isBlocked ? "ERT is blocked!" : "ERT is no longer blocked!";
        shell.WriteLine(message);
        _chatManager.SendAdminAnnouncement(message);
    }
}
