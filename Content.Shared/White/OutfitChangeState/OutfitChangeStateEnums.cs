using Robust.Shared.Serialization;

namespace Content.Shared.White.OutfitChangeState;

[Serializable]
[NetSerializable]
public enum OutfitChangeStatus : byte
{
    Standby,
    Active
}

[Serializable]
[NetSerializable]
public enum OutfitChangeVisuals : byte
{
    Status
}
