using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Server.White.AspectsSystem.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.White.AspectsSystem.Commands
{
    [AdminCommand(AdminFlags.Fun)]
    public sealed class ForceAspectCommand : IConsoleCommand
    {
        public string Command => "forceaspect";
        public string Description => "Принудительно форсит аспект по его ID.";
        public string Help => "forceaspect <aspectId>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var ticker = EntitySystem.Get<GameTicker>();
            if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("This can only be executed while the game is in the pre-round lobby.");
                return;
            }

            if (args.Length != 1)
            {
                shell.WriteError("Использование: forceaspect <aspectId>");
                return;
            }

            var aspectId = args[0];
            var aspectManager = EntitySystem.Get<AspectManager>();
            var result = aspectManager.ForceAspect(aspectId);
            shell.WriteLine(result);
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class DeForceAspectCommand : IConsoleCommand
    {
        public string Command => "deforceaspect";
        public string Description => "Дефорсит принудительно установленный аспект.";
        public string Help => "deforceaspect";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var ticker = EntitySystem.Get<GameTicker>();
            if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("This can only be executed while the game is in the pre-round lobby.");
                return;
            }

            var aspectManager = EntitySystem.Get<AspectManager>();
            var result = aspectManager.DeForceAspect();
            shell.WriteLine(result);
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class GetForcedAspectCommand : IConsoleCommand
    {
        public string Command => "getforcedaspect";
        public string Description => "Получает информацию о принудительно установленном аспекте.";
        public string Help => "getforcedaspect";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var ticker = EntitySystem.Get<GameTicker>();
            if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("This can only be executed while the game is in the pre-round lobby.");
                return;
            }

            var aspectManager = EntitySystem.Get<AspectManager>();
            var result = aspectManager.GetForcedAspect();
            shell.WriteLine(result);
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class ListAspectsCommand : IConsoleCommand
    {
        public string Command => "listaspects";
        public string Description => "Список всех доступных аспектов.";
        public string Help => "listaspects";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var aspectManager = EntitySystem.Get<AspectManager>();
            var aspectIds = aspectManager.GetAllAspectIds();

            if (aspectIds.Count == 0)
            {
                shell.WriteLine("Нет доступных аспектов.");
            }
            else
            {
                shell.WriteLine("Список доступных аспектов:");
                foreach (var aspectId in aspectIds)
                {
                    shell.WriteLine(aspectId);
                }
            }
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class RunAspectCommand : IConsoleCommand
    {
        public string Command => "runaspect";
        public string Description => "Запускает аспект по его ID.";
        public string Help => "runaspect <aspectId>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 1)
            {
                shell.WriteError("Использование: runaspect <aspectId>");
                return;
            }

            var aspectId = args[0];
            var aspectManager = EntitySystem.Get<AspectManager>();
            var result = aspectManager.RunAspect(aspectId);
            shell.WriteLine(result);
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class RunRandomAspectCommand : IConsoleCommand
    {
        public string Command => "runrandomaspect";
        public string Description => "Запускает случайный аспект.";
        public string Help => "runrandomaspect";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var aspectManager = EntitySystem.Get<AspectManager>();
            var result = aspectManager.RunRandomAspect();
            shell.WriteLine(result);
        }
    }
}
