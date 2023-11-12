using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Item.Tricorder;

[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedTricorderSystem))]
public sealed class TricorderComponent : Component
{
    [DataField("currentState"), ViewVariables(VVAccess.ReadWrite)]
    public TricorderMode CurrentMode = TricorderMode.Multitool;

    [DataField("soundSwitchMode")]
    public SoundSpecifier SoundSwitchMode = new SoundPathSpecifier("/Audio/Machines/quickbeep.ogg");
}

/// <summary>
/// Contains network state for TricorderComponent.
/// </summary>
[Serializable, NetSerializable]
public sealed class TricorderComponentState : ComponentState
{
    public TricorderMode CurrentMode;

    public TricorderComponentState(TricorderMode currentMode)
    {
        CurrentMode = currentMode;
    }
}

public enum TricorderMode
{
    Multitool,
    GasAnalyzer,
    HealthAnalyzer
}