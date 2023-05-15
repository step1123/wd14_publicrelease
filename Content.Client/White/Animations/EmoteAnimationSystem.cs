using Content.Shared.Animations;
using Robust.Client.Animations;
using Robust.Shared.Animations;
using Robust.Shared.GameStates;
using Robust.Client.GameObjects;
using static Content.Shared.Animations.EmoteAnimationComponent;
namespace Content.Client.Animations;

public class EmoteAnimationSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;
    private readonly Dictionary<string, Action<EntityUid>> _emoteList = new();
    //OnVerbsResponse?.Invoke(msg);
    public override void Initialize()
    {
        SubscribeLocalEvent<EmoteAnimationComponent, ComponentHandleState>(OnHandleState);

        // EmoteFlip animation
        _emoteList.Add("EmoteFlip", (EntityUid uid) =>
        {
            var animationKey = "emoteAnimationKeyId";

            if (_animationSystem.HasRunningAnimation(uid, animationKey))
                return;

            var baseAngle = Angle.Zero;
            if (EntityManager.TryGetComponent(uid, out SpriteComponent? sprite))
            {
                if (sprite != null)
                    baseAngle = sprite.Rotation;
            }

            var animation = new Animation
            {
                Length = TimeSpan.FromMilliseconds(500),
                AnimationTracks =
                {
                    new AnimationTrackComponentProperty
                    {
                        ComponentType = typeof(SpriteComponent),
                        Property = nameof(SpriteComponent.Rotation),
                        InterpolationMode = AnimationInterpolationMode.Linear,
                        KeyFrames =
                        {
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(baseAngle.Degrees), 0f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(baseAngle.Degrees + 180), 0.25f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(baseAngle.Degrees + 360), 0.25f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(baseAngle.Degrees), 0f),
                        }
                    }
                }
            };

            _animationSystem.Play(uid, animation, animationKey);
        });
        // EmoteJump animation
        _emoteList.Add("EmoteJump", (EntityUid uid) =>
        {
            var animationKey = "emoteAnimationKeyId";

            if (_animationSystem.HasRunningAnimation(uid, animationKey))
                return;

            var animation = new Animation
            {
                Length = TimeSpan.FromMilliseconds(250),
                AnimationTracks =
                {
                    new AnimationTrackComponentProperty
                    {
                        ComponentType = typeof(SpriteComponent),
                        Property = nameof(SpriteComponent.Offset),
                        InterpolationMode = AnimationInterpolationMode.Cubic,
                        KeyFrames =
                        {
                            new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                            new AnimationTrackProperty.KeyFrame(new Vector2(0, 1), 0.125f),
                            new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),
                        }
                    }
                }
            };

            _animationSystem.Play(uid, animation, animationKey);
        });
        // EmoteTurn animation
        _emoteList.Add("EmoteTurn", (EntityUid uid) =>
        {
            var animationKey = "emoteAnimationKeyId_rotate"; // it needs for only rotate anim

            if (_animationSystem.HasRunningAnimation(uid, animationKey))
                return;

            var animation = new Animation
            {
                Length = TimeSpan.FromMilliseconds(600),  // Пока пусть на 0.6 секунд. В идеале бы до 0.9 на 3 поворота
                AnimationTracks =
                {
                    new AnimationTrackComponentProperty
                    {
                        ComponentType = typeof(TransformComponent),
                        Property = nameof(TransformComponent.LocalRotation),
                        InterpolationMode = AnimationInterpolationMode.Linear,
                        KeyFrames =
                        {
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.075f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.075f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.075f),
                            new AnimationTrackProperty.KeyFrame(Angle.Zero, 0.075f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.075f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.075f),
                            new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.075f),
                            new AnimationTrackProperty.KeyFrame(Angle.Zero, 0.075f),
                        }
                    }
                }
            };

            _animationSystem.Play(uid, animation, animationKey);
        });
    }

    private void OnHandleState(EntityUid uid, EmoteAnimationComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not EmoteAnimationComponentState state)
            return;

        component.AnimationId = state.AnimationId;
        _emoteList[component.AnimationId].Invoke(uid);
    }
}
