using Robust.Shared.Serialization;

namespace Content.Shared.White.ServerEvent;

[Serializable, NetSerializable]
public sealed class EventStarted
{
    public string EventName;

    public EventStarted(string eventName)
    {
        EventName = eventName;
    }
}


[Serializable, NetSerializable]
public sealed class EventEnded
{
    public string EventName;

    public EventEnded(string eventName)
    {
        EventName = eventName;
    }
}
