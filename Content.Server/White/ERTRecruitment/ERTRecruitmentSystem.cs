using System.Linq;
using System.Numerics;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.Ghost.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Server.White.ServerEvent;
using Content.Shared.White.ServerEvent;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.White.ERTRecruitment;

public sealed class ERTRecruitmentSystem : EntitySystem
{
    public static string EventName = "ERTRecruitment";
    private ISawmill _logger = default!;
    private MapId? _mapId = null;

    public ResPath OutpostMap = new("/Maps/ERT/ERTStation.yml");
    public ResPath ShuttleMap = new("/Maps/Shuttles/emergency_box.yml");

    public EntityUid? Outpost;
    public EntityUid? Shuttle;
    public EntityUid? TargetStation;

    [Dependency] private readonly ServerEventSystem _event = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        _logger = Logger.GetSawmill(EventName);
        SubscribeLocalEvent<RoundStartAttemptEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartAttemptEvent ev)
    {
        var stations = _station.GetStations();
        if (stations.Count > 0)
        {
            TargetStation = stations[0];
        }

        SpawnMap();
    }

    private void AcceptERT()
    {
       if(TargetStation != null)
           _chatSystem.DispatchStationAnnouncement(TargetStation.Value,"ERT is accept! Please wait!",colorOverride: Color.Gold);
    }

    private void DeclineERT()
    {
        if(TargetStation != null)
            _chatSystem.DispatchStationAnnouncement(TargetStation.Value,"ERT is decline!",colorOverride: Color.Gold);
    }

    public void StartRecruitment(EntityUid? targetStation = null)
    {
        if (targetStation != null)
        {
            TargetStation = targetStation;
        }

        if(TargetStation == null)
            return;

        if (!_event.TryGetEvent(EventName, out var prototype) ||
            _event.GetEventSpawners(EventName).Count() < prototype.MinPlayer)
        {
            _logger.Error("Not enough spawners!");
            _event.BreakEvent(EventName);
            DeclineERT();
            return;
        }

        _chatSystem.DispatchStationAnnouncement(TargetStation.Value,"Please wait for a decision of ERT");

        if (TryComp<ShuttleComponent>(Shuttle, out var shuttle) && Outpost != null)
        {
            _shuttle.TryFTLDock(Shuttle.Value, shuttle, Outpost.Value);
            _shuttle.AddFTLDestination(TargetStation.Value, true);
        }

        var query = EntityQueryEnumerator<GhostComponent,ActorComponent>();
        while (query.MoveNext(out var uid,out _,out var actorComponent))
        {
            _eui.OpenEui(new ERTRecruitmentAcceptEui(uid,this),actorComponent.PlayerSession);
        }
    }

    public void Recruit(EntityUid uid)
    {
        if (!_event.TryGetEvent(EventName, out var prototype) )
            return;

        EnsureComp<ERTRecrutedComponent>(uid);
        _event.AddPlayer(uid,prototype);
    }

    private bool SpawnMap()
    {
        _logger.Debug("Loading maps!");
        if (_mapId != null)
            return true;

        var path = OutpostMap;
        var shuttlePath = ShuttleMap;

        var mapId = _mapManager.CreateMap();
        var options = new MapLoadOptions
        {
            LoadMap = true,
        };

        if (!_map.TryLoad(mapId, path.ToString(), out var outpostGrids, options) || outpostGrids.Count == 0)
        {
            _logger.Error( $"Error loading map {path}!");
            return false;
        }

        // Assume the first grid is the outpost grid.
        Outpost = outpostGrids[0];

        // Listen I just don't want it to overlap.
        if (!_map.TryLoad(mapId, shuttlePath.ToString(), out var grids, new MapLoadOptions {Offset = Vector2.One * 1000f}) || !grids.Any())
        {
            _logger.Error( $"Error loading grid {shuttlePath}!");
            return false;
        }

        var shuttleId = grids.First();

        // Naughty, someone saved the shuttle as a map.
        if (Deleted(shuttleId))
        {
            _logger.Error( $"Tried to load shuttle as a map, aborting.");
            _mapManager.DeleteMap(mapId);
            return false;
        }

        if (TryComp<ShuttleComponent>(shuttleId, out var shuttle))
        {
            _shuttle.TryFTLDock(shuttleId, shuttle, Outpost.Value);
        }

        _mapId = mapId;
        Shuttle = shuttleId;
        return true;
    }


    public void EndRecruitment()
    {
        if (!_event.TryGetEvent(EventName, out var prototype) )
            return;

        var query = EntityQueryEnumerator<ERTRecrutedComponent>();
        var go = prototype.PlayerCount >= prototype.MinPlayer;

        var spawners = _event.GetEventSpawners(EventName).ToList();
        _random.Shuffle(spawners);
        var count = 0;

        while (query.MoveNext(out var uid,out _))
        {
            RemComp<ERTRecrutedComponent>(uid);
            if(!TryComp<ActorComponent>(uid,out var actor) || count >= spawners.Count)
                continue;

            if (go)
            {
                var (spawnerUid, spawnerComponent) = spawners[count];

                _event.TransferMind(uid,spawnerUid,spawnerComponent);
                count++;
                _chat.DispatchServerMessage(actor.PlayerSession, prototype.Description);
            }
            else
            {
                _chat.DispatchServerMessage(actor.PlayerSession,"Not enough player for " + prototype.Name);
            }
        }

        if(go)
            AcceptERT();
        else
            DeclineERT();
    }
}
