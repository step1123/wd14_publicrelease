using System.Numerics;
using Content.Shared.Humanoid;
using Content.Shared.White.Cult;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.White.Cult;

public sealed class CultHudOverlay : Overlay
{

    private readonly IEntityManager _entityManager;
    private readonly SharedTransformSystem _transformSystem;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;



    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var spriteQuery = _entityManager.GetEntityQuery<SpriteComponent>();
        var xformQuery = _entityManager.GetEntityQuery<TransformComponent>();

        const float scale = 1f;
        var scaleMatrix = Matrix3.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3.CreateRotation(-rotation);

        foreach (var cultist in _entityManager.EntityQuery<CultistComponent>(true))
        {
            if (!xformQuery.TryGetComponent(cultist.Owner, out var xform) ||
                xform.MapID != args.MapId)
            {
                continue;
            }

            var worldPosition = _transformSystem.GetWorldPosition(xform);
            var worldMatrix = Matrix3.CreateTranslation(worldPosition);

            Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
            Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);

            handle.SetTransform(matty);

            var cultistIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/White/Cult/cult_hud.rsi"), "cult");
            var iconTexture = _entityManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(cultistIcon);

            float yOffset;
            float xOffset;

            if (spriteQuery.TryGetComponent(cultist.Owner, out var sprite))
            {
                yOffset = sprite.Bounds.Height - 10f; //sprite.Bounds.Height + 7f;
                xOffset = sprite.Bounds.Width - 40f; //sprite.Bounds.Width + 7f;
            }
            else
            {
                yOffset = 1f;
                xOffset = 1f;
            }

            // Position above the entity (we've already applied the matrix transform to the entity itself)
            // Offset by the texture size for every do_after we have.
            var position = new Vector2(xOffset / EyeManager.PixelsPerMeter, yOffset / EyeManager.PixelsPerMeter);

            // Draw the underlying bar texture
            if (sprite != null && !sprite.ContainerOccluded)
            {
                handle.DrawTexture(iconTexture, position);
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
    }

    public CultHudOverlay(IEntityManager entityManager)
    {
        _entityManager = entityManager;
        _transformSystem = _entityManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
    }
}
