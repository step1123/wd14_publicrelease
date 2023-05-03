using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.White.CPR.Events;

[Serializable, NetSerializable]
public sealed class CPREndedEvent : DoAfterEvent
{
    [DataField("user", required: true)]
    public readonly EntityUid User = default!;

    [DataField("target", required: true)]
    public readonly EntityUid Target = default!;

    private CPREndedEvent()
    {
    }

    public CPREndedEvent(EntityUid user, EntityUid target)
    {
        User = user;
        Target = target;
    }

    public override DoAfterEvent Clone() => this;
}
