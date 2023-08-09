using System.Diagnostics.CodeAnalysis;
using Content.Server.Humanoid.Components;
using Content.Server.Mind;
using Content.Server.Players;
using Content.Shared.White.ServerEvent;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.White.ServerEvent;

public sealed class ServerEventSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    private readonly Dictionary<string,ServerEventPrototype> _eventCache = new();

    public bool TryGetEvent(string eventName,[MaybeNullWhen(false)] out ServerEventPrototype prototype)
    {
        if (!_eventCache.TryGetValue(eventName, out prototype))
        {
            if (!_prototype.TryIndex(eventName, out prototype))
                return false;
            _eventCache.Add(eventName,prototype);
        }

        return true;
    }

    public bool IsEventRunning(string eventName)
    {
        if (!TryGetEvent(eventName, out var prototype))
            return false;
        return IsEventRunning(prototype);
    }

    private bool IsEventRunning(ServerEventPrototype prototype)
    {
        return prototype.EndPlayerGatherTime.HasValue;
    }

    public IEnumerable<ServerEventPrototype> GetActiveEvents()
    {
        foreach (var (_,prototype) in _eventCache)
        {
            if (prototype.EndPlayerGatherTime.HasValue)
                yield return prototype;
        }
    }

    public void BreakEvent(string eventName)
    {
        if(!TryGetEvent(eventName,out var prototype))
            return;
        BreakEvent(prototype);
    }

    private void BreakEvent(ServerEventPrototype prototype)
    {
        prototype.CurrentPlayerGatherTime = null;
        prototype.EndPlayerGatherTime = null;
        prototype.PlayerCount = 0;
    }

    public IEnumerable<(EntityUid, EventSpawnPointComponent)> GetEventSpawners(string eventName)
    {
        if (!TryGetEvent(eventName, out var prototype))
            yield break;

        var query = EntityQueryEnumerator<EventSpawnPointComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.EventType == eventName)
                yield return (uid, component);
        }
    }

    public void AddPlayer(EntityUid uid,ServerEventPrototype prototype)
    {
        prototype.PlayerCount += 1;
    }

    public bool TryStartEvent(string eventName)
    {
        if (!TryGetEvent(eventName, out var prototype) || IsEventRunning(prototype))
            return false;

        prototype.EndPlayerGatherTime = _timing.CurTime + prototype.PlayerGatherTime;
        return true;
    }

    public void TransferMind(EntityUid from,EntityUid spawnerUid,EventSpawnPointComponent? component = null)
    {
        if (!Resolve(spawnerUid, ref component) || !TryComp<ActorComponent>(from,out var actorComponent))
            return;

        var entityUid = Spawn(spawnerUid, component);

        if(!entityUid.HasValue)
            return;
        var mind = actorComponent.PlayerSession.GetMind()!;

        if (HasComp<RandomHumanoidSpawnerComponent>(entityUid.Value))
        {
            entityUid = new EntityUid((int) entityUid.Value + 1);
        }

        _mind.TransferTo(mind,entityUid.Value);
        _mind.UnVisit(mind);
    }

    public EntityUid? Spawn(EntityUid spawnerUid,EventSpawnPointComponent? component = null)
    {
        if (!Resolve(spawnerUid, ref component))
            return null;

        return EntityManager.SpawnEntity(component.EntityPrototype, Transform(spawnerUid).Coordinates);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var prototype in GetActiveEvents())
        {
            if (!prototype.CurrentPlayerGatherTime.HasValue)
            {
                prototype.OnStart?.Execute(prototype);
            }
            prototype.CurrentPlayerGatherTime = _timing.CurTime;

            if (prototype.CurrentPlayerGatherTime > prototype.EndPlayerGatherTime)
            {
                prototype.OnEnd?.Execute(prototype);
                BreakEvent(prototype);
            }
        }
    }
}
