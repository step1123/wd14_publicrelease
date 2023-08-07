using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Access.Components;
using Content.Shared.Roles;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.White.Mindshield;

namespace Content.Client.EntityJobInfo;

public sealed class MindShieldOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private string _hudPath = "/Textures/White/Overlays/MindShield/hud.rsi";

    public MindShieldOverlay(IEntityManager entManager)
    {
        _entManager = entManager;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var spriteQuery = _entManager.GetEntityQuery<SpriteComponent>();
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        const float scale = 1.15f;
        var scaleMatrix = Matrix3.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3.CreateRotation(-rotation);

        var entities = _entManager.EntityQuery<HumanoidAppearanceComponent, MindShieldComponent>();
        foreach (var (_, mindShieldComponent) in entities)
        {
            if (!xformQuery.TryGetComponent(mindShieldComponent.Owner, out var xform) ||
                xform.MapID != args.MapId)
            {
                continue;
            }

            var worldPosition = _transform.GetWorldPosition(xform);
            var worldMatrix = Matrix3.CreateTranslation(worldPosition);

            Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
            Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);

            handle.SetTransform(matty);

            var sprite_icon = new SpriteSpecifier.Rsi(new ResPath(_hudPath), "hui");
            var _iconTexture = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(sprite_icon);

            float yOffset;
            float xOffset;
            if (spriteQuery.TryGetComponent(mindShieldComponent.Owner, out var sprite))
            {
                yOffset = sprite.Bounds.Height - 26.5f; //sprite.Bounds.Height + 7f;
                xOffset = sprite.Bounds.Width - 14.8f; //sprite.Bounds.Width + 7f;
            }
            else
            {
                yOffset = 1f;
                xOffset = 1f;
            }

            var position = new Vector2(xOffset / EyeManager.PixelsPerMeter,
                yOffset / EyeManager.PixelsPerMeter);

            if (sprite != null && !sprite.ContainerOccluded)
                handle.DrawTexture(_iconTexture, position);
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
    }
}
