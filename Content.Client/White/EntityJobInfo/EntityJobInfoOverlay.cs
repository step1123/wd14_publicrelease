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

namespace Content.Client.White.EntityJobInfo;

public sealed class EntityJobInfoOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    private readonly IPrototypeManager _prototypeManager;
    private readonly InventorySystem _inventorySystem;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public EntityJobInfoOverlay(IEntityManager entManager, IPrototypeManager protoManager, InventorySystem inventorySystem)
    {
        _entManager = entManager;
        _prototypeManager = protoManager;
        _inventorySystem = inventorySystem;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var spriteQuery = _entManager.GetEntityQuery<SpriteComponent>();
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        const float scale = 1f;
        var scaleMatrix = Matrix3.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3.CreateRotation(-rotation);

        foreach (var hum in _entManager.EntityQuery<HumanoidAppearanceComponent>(true))
        {
            if (!xformQuery.TryGetComponent(hum.Owner, out var xform) ||
                xform.MapID != args.MapId)
            {
                continue;
            }

            var worldPosition = _transform.GetWorldPosition(xform);
            var worldMatrix = Matrix3.CreateTranslation(worldPosition);

            Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
            Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);

            handle.SetTransform(matty);

            var icon = GetIcon(hum.Owner) ?? "NoId"; //ATARAXIA-EDIT

            var sprite_icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/job_icons.rsi"), icon);
            var _iconTexture = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(sprite_icon);

            // Use the sprite itself if we know its bounds. This means short or tall sprites don't get overlapped
            // by the bar.
            float yOffset;
            float xOffset;
            if (spriteQuery.TryGetComponent(hum.Owner, out var sprite))
            {
                yOffset = sprite.Bounds.Height + 7f; //sprite.Bounds.Height + 7f;
                xOffset = sprite.Bounds.Width - 17f; //sprite.Bounds.Width + 7f;
            }
            else
            {
                yOffset = 1f;
                xOffset = 1f;
            }

            // Position above the entity (we've already applied the matrix transform to the entity itself)
            // Offset by the texture size for every do_after we have.
            var position = new Vector2(xOffset / EyeManager.PixelsPerMeter,
                yOffset / EyeManager.PixelsPerMeter);

            // Draw the underlying bar texture
            if (sprite != null && !sprite.ContainerOccluded)
                handle.DrawTexture(_iconTexture, position);
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
    }

    private string? GetIcon(EntityUid uid)
    {
        // ATARAXIA-EDIT
        if (!_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
            return null;

        // PDA
        if (_entManager.TryGetComponent<PdaComponent>(idUid, out var pda) && _entManager.TryGetComponent<IdCardComponent>(pda.ContainedId, out var id))
        {
            // Custom icon (ID card console)
            if (!string.IsNullOrWhiteSpace(id.CustomIcon))
                return id.CustomIcon;

            if (TryMatchJobTitleToIcon(id.JobTitle, out var icon))
                return icon;
        }
        // ID Card
        if (!_entManager.TryGetComponent(idUid, out id))
            return null;

        // Custom icon (ID card console)
        if (string.IsNullOrWhiteSpace(id.CustomIcon))
            return null;

        return TryMatchJobTitleToIcon(id.JobTitle, out var jobIcon) ? jobIcon : null;
        // ATARAXIA-EDIT
    }

    private bool TryMatchJobTitleToIcon(string? jobTitle, [NotNullWhen(true)] out string? jobIcon)
    {
        foreach (var job in _prototypeManager.EnumeratePrototypes<JobPrototype>())
        {
            if (job.LocalizedName == jobTitle)
            {
                jobIcon = job.Icon;
                return true;
            }
        }

        jobIcon = "CustomId";
        return true; // For 'CustomId' icon we need send 'true' result;
    }
}
