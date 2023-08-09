using Content.Server.Administration;
using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Enums;

namespace Content.Server.White.Commands;

[AdminCommand(AdminFlags.Admin)]
internal sealed class MOOCCommand : IConsoleCommand
{
    public string Command => "mooc";
    public string Description => "отправить OOC сообщение для текущей карты.";
    public string Help => "mooc <text>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not IPlayerSession player)
        {
            shell.WriteError("This command cannot be run from the server.");
            return;
        }

        if (player.AttachedEntity is not { Valid: true } entity)
            return;

        if (player.Status != SessionStatus.InGame)
            return;

        if (args.Length < 1)
            return;

        var message = string.Join(" ", args).Trim();
        if (string.IsNullOrEmpty(message))
            return;

        EntitySystem.Get<ChatSystem>().TrySendInGameOOCMessage(entity, message, InGameOOCChatType.Mooc, false, shell, player);
    }
}
