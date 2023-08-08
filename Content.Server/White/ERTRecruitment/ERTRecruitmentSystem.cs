using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Ghost.Components;
using Content.Server.Mind;
using Content.Server.Mind.Components;
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

        var query = EntityQueryEnumerator<GhostComponent>();
        while (query.MoveNext(out var uid,out _))
        {
            EnsureComp<ERTRecrutedComponent>(uid);
            _event.AddPlayer(uid,prototype);
        }
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
                var entityUid = _event.Spawn(spawnerUid, spawnerComponent);
                count += 1;

                _logger.Debug(entityUid + " " + uid);
                if(!entityUid.HasValue || !_mind.TryGetMind(uid,out var mind))
                    continue;

                _mind.TransferTo(mind,entityUid.Value);

                _chat.DispatchServerMessage(actor.PlayerSession, prototype.Description);
            }
            else
            {
                _chat.DispatchServerMessage(actor.PlayerSession,"Not enough player for " + prototype.Name);
            }
        }
    }
}
