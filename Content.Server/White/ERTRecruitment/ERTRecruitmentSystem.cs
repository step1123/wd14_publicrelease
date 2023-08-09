using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.EUI;
using Content.Server.Ghost.Components;
using Content.Server.Mind;
using Content.Server.White.ServerEvent;
using Content.Shared.White.ServerEvent;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.White.ERTRecruitment;

public sealed class ERTRecruitmentSystem : EntitySystem
{
    public static string EventName = "ERTRecruitment";
    private ISawmill _logger = default!;

    [Dependency] private readonly ServerEventSystem _event = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly EuiManager _eui = default!;

    public override void Initialize()
    {
        _logger = Logger.GetSawmill(EventName);
    }

    public void StartRecruitment()
    {
        if (!_event.TryGetEvent(EventName, out var prototype) ||
            _event.GetEventSpawners(EventName).Count() < prototype.MinPlayer)
        {
            _logger.Error("Not enough spawners!");
            _event.BreakEvent(EventName);
            return;
        }

        var query = EntityQueryEnumerator<GhostComponent,ActorComponent>();
        while (query.MoveNext(out var uid,out _,out var actorComponent))
        {
            _eui.OpenEui(new ERTRecruitmentAcceptEui(uid,this),actorComponent.PlayerSession);
        }
    }

    public void Recruit(EntityUid uid)
    {
        EnsureComp<ERTRecrutedComponent>(uid);
        if (!_event.TryGetEvent(EventName, out var prototype) )
            return;
        _event.AddPlayer(uid,prototype);
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
    }
}
