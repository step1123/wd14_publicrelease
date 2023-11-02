using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Content.Shared.White.Cult;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Enums;

namespace Content.Server.Chat.Commands
{
    [AnyCommand]
    internal sealed class CultCommand : IConsoleCommand
    {
        public string Command => "csay";
        public string Description => "Send cult message";
        public string Help => "csay <text>";

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

            var entMan = IoCManager.Resolve<IEntityManager>();
            if (!entMan.HasComponent<CultistComponent>(player.AttachedEntity))
                return;

            if (args.Length < 1)
                return;

            var message = string.Join(" ", args).Trim();
            if (string.IsNullOrEmpty(message))
                return;

            EntitySystem.Get<ChatSystem>().TrySendInGameOOCMessage(entity, message, InGameOOCChatType.Cult, false, shell, player);
        }
    }
}
