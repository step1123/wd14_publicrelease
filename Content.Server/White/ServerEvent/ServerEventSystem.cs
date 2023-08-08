using System.Diagnostics.CodeAnalysis;
using Content.Server.Humanoid.Components;
using Content.Shared.White.ServerEvent;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.White.ServerEvent;

public sealed class ServerEventSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

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

        prototype.CurrentPlayerGatherTime = null;
        prototype.EndPlayerGatherTime = null;
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
        if (!TryGetEvent(eventName, out var prototype) || IsEventRunning(eventName))
            return false;

        prototype.EndPlayerGatherTime = _timing.CurTime + prototype.PlayerGatherTime;
        return true;
    }

    public EntityUid? Spawn(EntityUid spawnerUid,EventSpawnPointComponent? component = null)
    {
        if (!Resolve(spawnerUid, ref component))
            return null;

        var uid = EntityManager.SpawnEntity(component.EntityPrototype, Transform(spawnerUid).Coordinates);

        return HasComp<RandomHumanoidSpawnerComponent>(uid) ? new EntityUid((int)uid + 1) : uid;
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
                prototype.CurrentPlayerGatherTime = null;
                prototype.EndPlayerGatherTime = null;
                prototype.PlayerCount = 0;
            }
        }
    }
}
