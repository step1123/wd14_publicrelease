using System.Diagnostics.CodeAnalysis;
using Content.Shared.White.ServerEvent;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.White.ServerEvent;

public sealed class ServerEventSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly Dictionary<string,ServerEventPrototype> _eventCache = new();

    public override void Initialize()
    {
        base.Initialize();


    }

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
        if (_eventCache.TryGetValue(eventName, out var prototype) || prototype == null)
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

    public bool TryStartEvent(string eventName)
    {
        if (!TryGetEvent(eventName, out var prototype) || IsEventRunning(eventName))
            return false;

        prototype.EndPlayerGatherTime = _timing.CurTime + prototype.PlayerGatherTime;
        return true;
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var prototype in GetActiveEvents())
        {
            if (!prototype.CurrentPlayerGatherTime.HasValue)
            {
                Logger.Debug("Start event " + prototype.Name );
            }
            prototype.CurrentPlayerGatherTime = _timing.CurTime;

            if (prototype.CurrentPlayerGatherTime > prototype.EndPlayerGatherTime)
            {
                Logger.Debug("End event " + prototype.Name );
                prototype.CurrentPlayerGatherTime = null;
                prototype.EndPlayerGatherTime = null;
            }
        }
    }
}
