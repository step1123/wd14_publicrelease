using System;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class CyborgTransformAnimationComponent : Component
{
    [ViewVariables] public const string AnimationKey = "Transform_animation";
    [DataField("enabled")] public bool Enabled = true;

    [DataField("transformState", required:true)]
    public string TransformState = default!;

    [DataField("beforeTransformState")]
    public string? BeforeTransformState;

    [DataField("duration"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan Duration = TimeSpan.FromSeconds(3.4);

    [ViewVariables] public List<int> HiddenLayers = new List<int>();
}

[Serializable, NetSerializable]
public sealed class CyborgTransformAnimationComponentState : ComponentState
{
    public bool Enabled;

    public CyborgTransformAnimationComponentState(bool enabled)
    {
        Enabled = enabled;
    }
}
