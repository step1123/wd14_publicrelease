using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Systems;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client.White.Cyborg;

public sealed class CyborgSystemTransformAnimation : SharedCyborgSystemTransformAnimation
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyborgTransformAnimationComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }


    private void OnAnimationCompleted(EntityUid uid, CyborgTransformAnimationComponent component, AnimationCompletedEvent args)
    {
        if(args.Key != CyborgTransformAnimationComponent.AnimationKey || !TryComp<SpriteComponent>(uid, out var spriteComponent))
            return;

        foreach (var count in component.HiddenLayers)
        {
            spriteComponent[count].Visible = true;
        }

        spriteComponent.LayerSetState(CyborgTransformVisualLayers.Main, component.BeforeTransformState);
    }

    protected override void PlayAnimation(EntityUid uid, CyborgTransformAnimationComponent component, DoAfterComponent? doAfterComponent = null)
    {
        base.PlayAnimation(uid, component);

        if (TryComp<SpriteComponent>(uid, out var spriteComponent) &&
            spriteComponent.LayerMapTryGet(CyborgTransformVisualLayers.Main,out var layerIndex))
        {
            var layers = spriteComponent.AllLayers.ToList();
            for (var i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];

                if (!layer.Visible)
                    continue;

                component.HiddenLayers.Add(i);
                layer.Visible = false;
            }

            spriteComponent[layerIndex].Visible = true;
            component.BeforeTransformState = spriteComponent.LayerGetState(layerIndex).Name;
        }

        var animation = new Animation
        {
            Length = component.Duration,
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick()
                {
                    LayerKey = CyborgTransformVisualLayers.Main,
                    KeyFrames = { new AnimationTrackSpriteFlick.KeyFrame(component.TransformState, 0f) }
                },
            }
        };

        _animationSystem.Play(uid,animation,CyborgTransformAnimationComponent.AnimationKey);
    }
}


public enum CyborgTransformVisualLayers : byte
{
    Main
}
