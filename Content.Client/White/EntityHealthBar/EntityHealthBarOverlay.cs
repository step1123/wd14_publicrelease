using System.Numerics;
using System.Threading;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Client.White.EntityHealthBar;

/// <summary>
/// Yeah a lot of this is duplicated from doafters.
/// Not much to be done until there's a generic HUD system
/// </summary>
public sealed class EntityHealthBarOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly SpriteSystem _spriteSystem;
    private readonly SharedTransformSystem _transform;
    private readonly MobThresholdSystem _mobThresholdSystem;

    private readonly Texture _barTexture;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public string? DamageContainer;

    // for icon frame change timer
    private int _iconFrame = 1;
    private const double DelayTime = 0.25;

    public EntityHealthBarOverlay(IEntityManager entManager)
    {
        _entManager = entManager;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _mobThresholdSystem = _entManager.EntitySysManager.GetEntitySystem<MobThresholdSystem>();
        _spriteSystem = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>();

        var sprite = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/health_status.rsi"), "background");
        _barTexture = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(sprite);

        Timer.SpawnRepeating(TimeSpan.FromSeconds(DelayTime), () =>
        {
            if (_iconFrame < 8)
                _iconFrame++;
            else
                _iconFrame = 1;
        }, new CancellationToken());
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var spriteQuery = _entManager.GetEntityQuery<SpriteComponent>();
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();
        var mobsQuery = _entManager.EntityQuery<MobStateComponent, DamageableComponent>();

        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;

        var scaleMatrix = Matrix3.CreateScale(Vector2.One);
        var rotationMatrix = Matrix3.CreateRotation(-rotation);

        // Hardcoded width of the progress bar because it doesn't match the texture.
        const float startX = 1f;
        const float endX = 15f;

        foreach (var (mob, dmg) in mobsQuery)
        {
            if (DamageContainer != null && dmg.DamageContainerID != DamageContainer)
                continue;

            if (!xformQuery.TryGetComponent(mob.Owner, out var xform) || xform.MapID != args.MapId)
                continue;

            var worldPosition = _transform.GetWorldPosition(xform);
            var worldMatrix = Matrix3.CreateTranslation(worldPosition);
            Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
            Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);

            handle.SetTransform(matty);

            // Use the sprite itself if we know its bounds. This means short or tall sprites don't get overlapped
            // by the bar.
            var yOffset = 1f;
            var xIconOffset = 1f;
            var yIconOffset = 1f;
            if (spriteQuery.TryGetComponent(mob.Owner, out var sprite))
            {
                yOffset = sprite.Bounds.Height + 16f;
                yIconOffset = sprite.Bounds.Height + 13f;
                xIconOffset = sprite.Bounds.Width + 7f;
            }

            // Position above the entity (we've already applied the matrix transform to the entity itself)
            // Offset by the texture size for every do_after we have.
            var position = new Vector2(
                -_barTexture.Width / 2f / EyeManager.PixelsPerMeter,
                yOffset / EyeManager.PixelsPerMeter
            );

            // Draw the underlying bar texture
            if (sprite == null || sprite.ContainerOccluded)
                continue;

            handle.DrawTexture(_barTexture, position);

            // Draw state icon
            var currentState = mob.CurrentState switch
            {
                MobState.Alive    => "life_state",
                MobState.Critical => "defib_state",
                MobState.Dead     => "dead_state",
                _                 => "defib_state"
            };

            var iconSprite =
                new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/health_state.rsi"), currentState);

            var stateIcon = _spriteSystem.RsiStateLike(iconSprite)
                .GetFrame(0, GetIconFrame(_spriteSystem.RsiStateLike(iconSprite)));

            var iconPosition = new Vector2(xIconOffset / EyeManager.PixelsPerMeter,
                yIconOffset / EyeManager.PixelsPerMeter);

            handle.DrawTexture(stateIcon, iconPosition);

            // we are all progressing towards death every day
            var deathProgress = CalculateProgress(mob.Owner, mob, dmg);
            var color = GetProgressColor(deathProgress, mob.CurrentState == MobState.Critical);

            var xProgress = (endX - startX) * deathProgress + startX;

            var box = new Box2(new Vector2(startX, 0f) / EyeManager.PixelsPerMeter,
                new Vector2(xProgress, 2f) / EyeManager.PixelsPerMeter);

            box = box.Translated(position);
            handle.DrawRect(box, color);
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
    }

    private int GetIconFrame(IRsiStateLike sprite)
    {
        if (sprite.AnimationFrameCount <= 1)
            return 0;

        var currentFrame = _iconFrame;
        int result;
        while (true)
        {
            if (currentFrame <= 0 || currentFrame <= sprite.AnimationFrameCount)
            {
                result = currentFrame - 1;
                break;
            }

            currentFrame -= sprite.AnimationFrameCount;
        }

        return result;
    }

    /// <summary>
    /// Returns a ratio between 0 and 1, and whether the entity is in crit.
    /// </summary>
    private float CalculateProgress(EntityUid uid, MobStateComponent component, DamageableComponent dmg)
    {
        return component.CurrentState switch
        {
            MobState.Alive    => GetAliveDamageState(uid, dmg),
            MobState.Critical => GetCriticalDamageState(uid, dmg),
            MobState.Dead     => 0,
            _                 => 1
        };
    }

    private float GetCriticalDamageState(EntityUid uid, DamageableComponent dmg)
    {
        if (_mobThresholdSystem.TryGetThresholdForState(uid, MobState.Critical, out var critThreshold) &&
            _mobThresholdSystem.TryGetThresholdForState(uid, MobState.Dead, out var deadThreshold))
        {
            return 1 - ((dmg.TotalDamage - critThreshold) / (deadThreshold - critThreshold)).Value.Float();
        }

        return 1f;
    }

    private float GetAliveDamageState(EntityUid uid, DamageableComponent dmg)
    {
        if (_mobThresholdSystem.TryGetThresholdForState(uid, MobState.Critical, out var threshold) ||
            _mobThresholdSystem.TryGetThresholdForState(uid, MobState.Dead, out threshold))
        {
            return 1 - (dmg.TotalDamage / threshold.Value).Float();
        }

        return 1;
    }

    private static Color GetProgressColor(float progress, bool crit)
    {
        if (crit)
        {
            return Color.Red;
        }

        if (progress >= 1.0f)
        {
            return new Color(0f, 1f, 0f);
        }

        // lerp
        var hue = 5f / 18f * progress;
        return Color.FromHsv((hue, 1f, 0.75f, 1f));
    }
}
