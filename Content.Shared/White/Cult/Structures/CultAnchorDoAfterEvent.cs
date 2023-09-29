using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cult.Structures;

[Serializable, NetSerializable]
public sealed class CultAnchorDoAfterEvent : SimpleDoAfterEvent
{
    public bool IsAnchored;

    public CultAnchorDoAfterEvent(bool isAnchored)
    {
        IsAnchored = isAnchored;
    }
}
