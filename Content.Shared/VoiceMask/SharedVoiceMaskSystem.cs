using Robust.Shared.Serialization;

namespace Content.Shared.VoiceMask;

[Serializable, NetSerializable]
public enum VoiceMaskUIKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class VoiceMaskBuiState : BoundUserInterfaceState
{
    public string Name { get; }
    public string Voice { get; }

    public VoiceMaskBuiState(string name, string voice)
    {
        Name = name;
        Voice = voice;
    }
}

[Serializable, NetSerializable]
public sealed class VoiceMaskChangeNameMessage : BoundUserInterfaceMessage
{
    public string Name { get; }

    public VoiceMaskChangeNameMessage(string name)
    {
        Name = name;
    }
}
