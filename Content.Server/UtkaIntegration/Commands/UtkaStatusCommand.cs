using System.Linq;
using System.Net;
using Content.Server.Administration.Managers;
using Content.Server.AlertLevel;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaStatusCommand : IUtkaCommand
{
    public string Name => "status";
    public Type RequestMessageType => typeof(UtkaStatusRequsets);

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly UtkaTCPWrapper _utkaSocketWrapper = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameMapManager _gameMapManager = default!;

    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if(baseMessage is not UtkaStatusRequsets message) return;
        var _gameTicker = EntitySystem.Get<GameTicker>();
        var _roundEndSystem = EntitySystem.Get<RoundEndSystem>();
        var _station = EntitySystem.Get<StationSystem>();
        IoCManager.InjectDependencies(this);

        var players = Filter.GetAllPlayers().ToList().Count;

        var admins = _adminManager.ActiveAdmins.Select(x => x.Name).ToList().Count;

        string shuttleData = string.Empty;

        if (_roundEndSystem.ExpectedCountdownEnd == null)
        {
            shuttleData = "idle";
        }
        else
        {
            shuttleData = "called";
        }

        var roundDuration = _gameTicker.RoundDuration().TotalSeconds;

        string? gameMap = null;
        string? stationCode = null;
        foreach (var station in _station.GetStations())
        {
            if (!_entMan.TryGetComponent(station, out AlertLevelComponent? alert) || stationCode != null)
            {
                continue;
            }

            if (alert is { CurrentLevel: { } })
            {
                stationCode = alert.CurrentLevel;

                var map = _gameMapManager.GetSelectedMap();
                gameMap = map?.MapName ?? Loc.GetString("discord-round-unknown-map");
            }
        }

        var toUtkaMessage = new UtkaStatusResponse()
        {
            Players = players,
            Admins = admins,
            Map = gameMap,
            ShuttleStatus = shuttleData,
            RoundDuration = roundDuration,
            StationCode = stationCode
        };

        _utkaSocketWrapper.SendMessageToAll(toUtkaMessage);
    }
}
