using Robust.Shared.GameStates;
using static Content.Shared.Animations.EmoteAnimationComponent;
using Robust.Shared.Prototypes;

namespace Content.Shared.Animations;

public class SharedEmoteAnimationSystem : EntitySystem
{
    public const string EmoteFlipActionPrototype = "EmoteFlip";
    public const string EmoteJumpActionPrototype = "EmoteJump";
    public const string EmoteTurnActionPrototype = "EmoteTurn";

    [Dependency] public readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] public readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
    }
}
