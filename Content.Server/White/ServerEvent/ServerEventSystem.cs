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

    private readonly Dictionary<string,ServerEventPrototype> _eventCache = new();

    public override void Shutdown()
    {
        _eventCache.Clear();
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
        if (!TryGetEvent(eventName, out var prototype))
            return false;

        return IsEventRunning(prototype);
    }

    private bool IsEventRunning(ServerEventPrototype prototype)
    {
        return prototype.EndPlayerGatherTime.HasValue && !prototype.IsBreak;
    }

    public IEnumerable<ServerEventPrototype> GetActiveEvents()
    {
        foreach (var (_,prototype) in _eventCache)
        {
            if (IsEventRunning(prototype))
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
        prototype.IsBreak = true;
    }

    public bool TryStartEvent(string eventName)
    {
        if (!TryGetEvent(eventName, out var prototype) || IsEventRunning(prototype))
            return false;

        prototype.IsBreak = false;
        prototype.EndPlayerGatherTime = _timing.CurTime + prototype.PlayerGatherTime;
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var prototype in GetActiveEvents())
        {
            Logger.Debug(prototype.CurrentPlayerGatherTime + " " + prototype.EndPlayerGatherTime + " " + prototype.IsBreak );
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
