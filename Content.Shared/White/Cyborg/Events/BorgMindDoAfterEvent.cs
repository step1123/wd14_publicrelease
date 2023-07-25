using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Events;

[Serializable, NetSerializable]
public sealed class BorgMindDoAfterEvent : SimpleDoAfterEvent
{
}
