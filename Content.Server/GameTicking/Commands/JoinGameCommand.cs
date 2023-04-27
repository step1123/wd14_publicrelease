using Content.Server.Chat.Managers;
using Content.Server.Station.Systems;
using Content.Server.White.Stalin;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Content.Shared.White;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Commands
{
    [AnyCommand]
    sealed class JoinGameCommand : IConsoleCommand
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StalinManager _stalinManager = default!;
        [Dependency] private readonly IConfigurationManager _configurationManager = default!;

        public string Command => "joingame";
        public string Description => "";
        public string Help => "";

        public JoinGameCommand()
        {
            IoCManager.InjectDependencies(this);
        }
        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var player = shell.Player as IPlayerSession;

            if (player == null)
            {
                return;
            }

            var ticker = EntitySystem.Get<GameTicker>();
            var stationSystem = EntitySystem.Get<StationSystem>();
            var stationJobs = EntitySystem.Get<StationJobsSystem>();

            if (ticker.PlayerGameStatuses.TryGetValue(player.UserId, out var status) && status == PlayerGameStatus.JoinedGame)
            {
                Logger.InfoS("security", $"{player.Name} ({player.UserId}) attempted to latejoin while in-game.");
                shell.WriteError($"{player.Name} is not in the lobby.   This incident will be reported.");
                return;
            }

            var chatManager = IoCManager.Resolve<IChatManager>();

            if (_configurationManager.GetCVar(WhiteCVars.StalinEnabled))
            {
                var allowEnterRequest = await _stalinManager.AllowEnter(player);

                if (!allowEnterRequest.allow)
                {
                    chatManager.DispatchServerMessage(player, allowEnterRequest.errorMessage);
                    return;
                }
            }

            if (ticker.RunLevel == GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("Round has not started.");
                return;
            }
            else if (ticker.RunLevel == GameRunLevel.InRound)
            {
                string id = args[0];

                if (!int.TryParse(args[1], out var sid))
                {
                    shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
                }

                var station = new EntityUid(sid);
                var jobPrototype = _prototypeManager.Index<JobPrototype>(id);
                if(stationJobs.TryGetJobSlot(station, jobPrototype, out var slots) == false || slots == 0)
                {
                    shell.WriteLine($"{jobPrototype.LocalizedName} has no available slots.");
                    return;
                }
                ticker.MakeJoinGame(player, station, id);
                return;
            }

            ticker.MakeJoinGame(player, EntityUid.Invalid);
        }
    }
}
