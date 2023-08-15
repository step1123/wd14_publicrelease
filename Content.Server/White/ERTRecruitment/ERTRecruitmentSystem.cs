using System.Linq;
using System.Numerics;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Server.White.GhostRecruitment;
using Content.Server.White.ServerEvent;
using Content.Shared.GameTicking;
using Content.Shared.White.GhostRecruitment;
using Content.Shared.White.ServerEvent;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server.White.ERTRecruitment;

public sealed class ERTRecruitmentSystem : EntitySystem
{
    public static string EventName = "ERTRecruitment";
    private ISawmill _logger = default!;
    private MapId? _mapId = null;

    public ResPath OutpostMap = new("/Maps/ERT/ERTStation.yml");
    public ResPath ShuttleMap = new("/Maps/ERT/ERTShuttle.yml");

    public SoundSpecifier ERTYes = new SoundPathSpecifier("/Audio/Announcements/ert_yes.ogg");
    public SoundSpecifier ERTNo = new SoundPathSpecifier("/Audio/Announcements/ert_no.ogg");

    public bool IsBlocked = false;

    public EntityUid? Outpost;
    public EntityUid? Shuttle;
    public EntityUid? TargetStation;

    [Dependency] private readonly ServerEventSystem _event = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly GhostRecruitmentSystem _recruitment = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        _logger = Logger.GetSawmill(EventName);
        SubscribeLocalEvent<RoundStartAttemptEvent>(OnRoundStart);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
        SubscribeLocalEvent<RecruitedComponent,GhostRecruitmentSuccessEvent>(OnRecruitmentSuccess);

        SubscribeLocalEvent<EventStarted>(OnEventStart);
        SubscribeLocalEvent<EventEnded>(OnEventEnd);
    }

    private void OnEventEnd(EventEnded ev)
    {
        if(ev.EventName != EventName)
            return;
        EventEnd();
    }

    private void OnEventStart(EventStarted ev)
    {
        if(ev.EventName != EventName)
            return;
        EventStart();
    }

    private void OnRoundEnd(RoundRestartCleanupEvent ev)
    {
        Outpost = null;
        Shuttle = null;
        TargetStation = null;

        _mapId = null;
        _logger.Debug("Deleted map");
    }

    private void OnRecruitmentSuccess(EntityUid uid, RecruitedComponent component, GhostRecruitmentSuccessEvent args)
    {
        if(args.RecruitmentName != EventName || !_event.TryGetEvent(EventName, out var prototype))
            return;

        _chat.DispatchServerMessage(args.PlayerSession,Loc.GetString("ert-description"));
        _chat.DispatchServerMessage(args.PlayerSession, Loc.GetString("ert-reason",("reason",prototype.Description)));
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

    public void EventStart()
    {
        if(TargetStation == null || IsBlocked)
            return;

        if (!_event.TryGetEvent(EventName, out var prototype) ||
            _recruitment.GetEventSpawners(EventName).Count() < prototype.MinPlayer)
        {
            _logger.Error("Not enough spawners!");
            _event.BreakEvent(EventName);
            DeclineERT();
            return;
        }

        _chatSystem.DispatchStationAnnouncement(TargetStation.Value,Loc.GetString("ert-wait-message"),colorOverride: Color.Gold);

        if (TryComp<ShuttleComponent>(Shuttle, out var shuttle) && Outpost != null)
        {
            _shuttle.TryFTLDock(Shuttle.Value, shuttle, Outpost.Value);
        }

        _recruitment.StartRecruitment(EventName);
    }

    public void EventEnd()
    {
        if (!_event.TryGetEvent(EventName, out var prototype) )
            return;

        if (_recruitment.GetAllRecruited(EventName).Count() < prototype.MinPlayer ||
            !_recruitment.EndRecruitment(EventName))
        {
            DeclineERT();
            return;
        }

        AcceptERT();
    }

    public void AcceptERT()
    {
       if(TargetStation == null)
           return;

       _chatSystem.DispatchStationAnnouncement(TargetStation.Value,Loc.GetString("ert-accept-message"),
           colorOverride: Color.Gold,announcementSound:ERTYes);
    }

    public void DeclineERT()
    {
        if(TargetStation == null)
            return;

        _chatSystem.DispatchStationAnnouncement(TargetStation.Value,Loc.GetString("ert-deny-message"),
            colorOverride: Color.Gold,announcementSound:ERTNo);
    }

    private bool SpawnMap()
    {
        _logger.Debug($"Loading maps!");
        if (_mapId != null)
        {
            // The map is already loaded, so there is no point in loading further
            _logger.Debug("Map is already loaded " + _mapId);
            return true;
        }

        var mapId = _mapManager.CreateMap();
        var options = new MapLoadOptions
        {
            LoadMap = true,
        };

        if (!_map.TryLoad(mapId, OutpostMap.ToString(), out var outpostGrids, options) || outpostGrids.Count == 0)
        {
            _logger.Error( $"Error loading map {OutpostMap}!");
            return false;
        }
        _logger.Debug($"Loaded map {OutpostMap} on {mapId}!");

        // Assume the first grid is the outpost grid.
        Outpost = outpostGrids[0];

        // Listen I just don't want it to overlap.
        if (!_map.TryLoad(mapId, ShuttleMap.ToString(), out var grids, new MapLoadOptions {Offset = Vector2.One * 1000f}) || !grids.Any())
        {
            _logger.Error( $"Error loading grid {ShuttleMap}!");
            return false;
        }
        _logger.Debug($"Loaded shuttle {ShuttleMap} on {mapId}!");

        var shuttleId = grids.First();

        // Naughty, someone saved the shuttle as a map.
        if (Deleted(shuttleId))
        {
            _logger.Error( $"Tried to load shuttle as a map, aborting.");
            _mapManager.DeleteMap(mapId);
            return false;
        }

        _mapId = mapId;
        Shuttle = shuttleId;
        return true;
    }



}
