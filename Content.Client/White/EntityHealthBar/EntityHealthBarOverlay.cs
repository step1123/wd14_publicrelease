using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.FixedPoint;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Client.White.EntityHealthBar;

/// <summary>
/// Yeah a lot of this is duplicated from doafters.
/// Not much to be done until there's a generic HUD system
/// </summary>
public sealed class EntityHealthBarOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    private readonly MobStateSystem _mobStateSystem;
    private readonly MobThresholdSystem _mobThresholdSystem;
    private readonly Texture _barTexture;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;
    public string? DamageContainer;
    // for icon frame change timer
    int iconFrame = 1;
    double delayTime = 0.25;

    public EntityHealthBarOverlay(IEntityManager entManager)
    {
        _entManager = entManager;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _mobStateSystem = _entManager.EntitySysManager.GetEntitySystem<MobStateSystem>();
        _mobThresholdSystem = _entManager.EntitySysManager.GetEntitySystem<MobThresholdSystem>();

        var sprite = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/health_status.rsi"), "background");
        _barTexture = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(sprite);

        Timer.SpawnRepeating(TimeSpan.FromSeconds(delayTime), () => {
            if (iconFrame < 8)
                iconFrame++;
            else
                iconFrame = 1;
        }, new System.Threading.CancellationToken());
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var spriteQuery = _entManager.GetEntityQuery<SpriteComponent>();
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        var _spriteSys = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>();

        const float scale = 1f;
        var scaleMatrix = Matrix3.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3.CreateRotation(-rotation);

        foreach (var (mob, dmg, threasholds) in _entManager.EntityQuery<MobStateComponent, DamageableComponent, MobThresholdsComponent>(true))
        {
            if (!xformQuery.TryGetComponent(mob.Owner, out var xform) ||
                xform.MapID != args.MapId)
            {
                continue;
            }

            if (DamageContainer != null && dmg.DamageContainerID != DamageContainer)
                continue;

            var worldPosition = _transform.GetWorldPosition(xform);
            var worldMatrix = Matrix3.CreateTranslation(worldPosition);

            Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
            Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);

            handle.SetTransform(matty);

            // Use the sprite itself if we know its bounds. This means short or tall sprites don't get overlapped
            // by the bar.
            float yOffset;
            float xIconOffset;
            float yIconOffset;
            if (spriteQuery.TryGetComponent(mob.Owner, out var sprite))
            {
                yOffset = sprite.Bounds.Height + 12f;
                yIconOffset = sprite.Bounds.Height + 7f;
                xIconOffset = sprite.Bounds.Width + 7f;
            }
            else
            {
                yOffset = 1f;
                yIconOffset = 1f;
                xIconOffset = 1f;
            }

            // Position above the entity (we've already applied the matrix transform to the entity itself)
            // Offset by the texture size for every do_after we have.
            var position = new Vector2(-_barTexture.Width / 2f / EyeManager.PixelsPerMeter,
                yOffset / EyeManager.PixelsPerMeter);

            // Draw the underlying bar texture
            if (sprite != null && !sprite.ContainerOccluded)
                handle.DrawTexture(_barTexture, position);
            else
                continue;

            // Draw state icon
            string current_state;
            if (_mobStateSystem.IsAlive(mob.Owner, mob))
            {
                current_state = "life_state";
            }
            else
            {
                if (_mobStateSystem.IsCritical(mob.Owner, mob) && _mobThresholdSystem.TryGetThresholdForState(mob.Owner, MobState.Critical, out var critThreshold))
                    current_state = "defib_state";
                else
                    current_state = "dead_state";
            }

            var icon_sprite = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/health_state.rsi"), current_state);
            Texture _stateIcon = _spriteSys.RsiStateLike(icon_sprite).GetFrame(0, GetIconFrame(_spriteSys.RsiStateLike(icon_sprite)));

            var icon_position = new Vector2(xIconOffset / EyeManager.PixelsPerMeter,
                yIconOffset / EyeManager.PixelsPerMeter);

            handle.DrawTexture(_stateIcon, icon_position);

            // we are all progressing towards death every day
            (float ratio, bool inCrit) deathProgress = CalcProgress(mob.Owner, mob, dmg);

            var color = GetProgressColor(deathProgress.ratio, deathProgress.inCrit);

            // Hardcoded width of the progress bar because it doesn't match the texture.
            const float startX = 1f;
            const float endX = 15f;

            var xProgress = (endX - startX) * deathProgress.ratio + startX;

            var box = new Box2(new Vector2(startX, 0f) / EyeManager.PixelsPerMeter, new Vector2(xProgress, 2f) / EyeManager.PixelsPerMeter);
            box = box.Translated(position);
            handle.DrawRect(box, color);
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
    }

    private int GetIconFrame(IRsiStateLike sprite)
    {
        var _spriteSys = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>();

        if (sprite.AnimationFrameCount <= 1)
            return 0;

        var currentFrame = iconFrame;
        var result = 0;
        while (true)
        {
            if (currentFrame > 0 && currentFrame > sprite.AnimationFrameCount)
            {
                currentFrame -= sprite.AnimationFrameCount;
            }
            else
            {
                result = currentFrame - 1;
                break;
            }
        }
        return result;
    }

    /// <summary>
    /// Returns a ratio between 0 and 1, and whether the entity is in crit.
    /// </summary>
    private (float, bool) CalcProgress(EntityUid uid, MobStateComponent component, DamageableComponent dmg)
    {
        if (_mobStateSystem.IsAlive(uid, component))
        {
            if (!_mobThresholdSystem.TryGetThresholdForState(uid, MobState.Critical, out var threshold))
                return (1, false);

            var ratio = 1 - ((FixedPoint2) (dmg.TotalDamage / threshold)).Float();
            return (ratio, false);
        }

        if (_mobStateSystem.IsCritical(uid, component))
        {
            if (!_mobThresholdSystem.TryGetThresholdForState(uid, MobState.Critical, out var critThreshold) ||
                !_mobThresholdSystem.TryGetThresholdForState(uid, MobState.Dead, out var deadThreshold))
            {
                return (1, true);
            }

            var ratio = 1 -
                    ((dmg.TotalDamage - critThreshold) /
                    (deadThreshold - critThreshold)).Value.Float();

            return (ratio, true);
        }

        return (0, true);
    }

    public static Color GetProgressColor(float progress, bool crit)
    {
        if (progress >= 1.0f)
        {
            return new Color(0f, 1f, 0f);
        }
        // lerp
        if (!crit)
        {
            var hue = (5f / 18f) * progress;
            return Color.FromHsv((hue, 1f, 0.75f, 1f));
        }
        else
        {
            return Color.Red;
        }
    }
}
