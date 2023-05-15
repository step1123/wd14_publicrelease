using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Animations;

/// <summary>
/// Event for playing animations
/// </summary>
public class EmoteActionEvent : InstantActionEvent
{
    [ViewVariables]
    [DataField("emote", readOnly: true, required: true)]
    public string Emote = default!;
};

[RegisterComponent]
[NetworkedComponent]
public class EmoteAnimationComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public string AnimationId = "none";

    public readonly List<InstantAction> Actions = new();

    [Serializable, NetSerializable]
    public class EmoteAnimationComponentState : ComponentState
    {
        public string AnimationId { get; init; }

        public EmoteAnimationComponentState(string animationId)
        {
            AnimationId = animationId;
        }
    }
}
