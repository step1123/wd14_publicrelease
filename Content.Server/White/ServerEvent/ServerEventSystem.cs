using System.Diagnostics.CodeAnalysis;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.White.ServerEvent;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.White.ServerEvent;

public sealed class ServerEventSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private readonly Dictionary<string,ServerEventPrototype> _eventCache = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnEnd);
    }

    private void OnEnd(RoundRestartCleanupEvent ev)
    {
        foreach (var eventPrototype in GetActiveEvents())
        {
            BreakEvent(eventPrototype);
        }

        _eventCache.Clear();
    }

    public bool TryGetEvent(string eventName,[MaybeNullWhen(false)] out ServerEventPrototype prototype)
    {
        if (!_eventCache.TryGetValue(eventName, out prototype))
        {
            if (!_prototype.TryIndex(eventName, out prototype))
            {
                Logger.Debug("Failed load " + eventName);
                return false;
            }
            Logger.Debug("Caching " + eventName);
            _eventCache.Add(eventName,prototype);
        }

        Logger.Debug("Successful get " + eventName);
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
        Logger.Debug("Event is break! " + prototype.ID);
        prototype.CurrentPlayerGatherTime = null;
        prototype.EndPlayerGatherTime = null;
        prototype.IsBreak = true;
    }

    public bool TryStartEvent(string eventName)
    {
        if (!TryGetEvent(eventName, out var prototype) || IsEventRunning(prototype) || _gameTicker.RunLevel != GameRunLevel.InRound)
            return false;

        prototype.IsBreak = false;
        prototype.EndPlayerGatherTime = _timing.CurTime + prototype.PlayerGatherTime;

        Logger.Debug("Event is started " + eventName);
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var prototype in GetActiveEvents())
        {
            if (!prototype.CurrentPlayerGatherTime.HasValue)
            {
                Logger.Debug("Started with " + prototype.CurrentPlayerGatherTime + " " + prototype.EndPlayerGatherTime + " " + prototype.IsBreak );
                prototype.OnStart?.Execute(prototype);
            }
            prototype.CurrentPlayerGatherTime = _timing.CurTime;

            if (prototype.CurrentPlayerGatherTime > prototype.EndPlayerGatherTime)
            {
                prototype.OnEnd?.Execute(prototype);
                Logger.Debug("Ended with " + prototype.CurrentPlayerGatherTime + " " + prototype.EndPlayerGatherTime + " " + prototype.IsBreak );
                BreakEvent(prototype);
            }
        }
    }
}
