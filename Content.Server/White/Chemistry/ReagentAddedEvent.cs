using Content.Shared.FixedPoint;

namespace Content.Server.White.Chemistry;

public sealed class ReagentAddedEvent : EntityEventArgs
{
    public FixedPoint2 Accepted;

    public ReagentAddedEvent(FixedPoint2 accepted)
    {
        Accepted = accepted;
    }
}

public sealed class ReagentWillAddedEvent : HandledEntityEventArgs
{
    public ReagentWillAddedEvent()
    {
    }
}
