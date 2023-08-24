using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Components;


[Serializable, NetSerializable]
public sealed class HypoSprayDoAfterEvent : SimpleDoAfterEvent
{
    public Solution HypoSpraySolution = default!;
    public Solution TargetSolution = default!;
    public FixedPoint2 RealTransferAmount;
}

[NetworkedComponent()]
public abstract class SharedHyposprayComponent : Component
{
    [DataField("solutionName")]
    public string SolutionName = "hypospray";
}

[Serializable, NetSerializable]
public sealed class HyposprayComponentState : ComponentState
{
    public FixedPoint2 CurVolume { get; }
    public FixedPoint2 MaxVolume { get; }

    public HyposprayComponentState(FixedPoint2 curVolume, FixedPoint2 maxVolume)
    {
        CurVolume = curVolume;
        MaxVolume = maxVolume;
    }
}
