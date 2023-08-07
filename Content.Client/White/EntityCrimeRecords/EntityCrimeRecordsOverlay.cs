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
using Content.Shared.White.CriminalRecords;
using Robust.Shared.Map;

namespace Content.Client.White.EntityCrimeRecords;

public sealed class EntityCrimeRecordsOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    private readonly InventorySystem _inventorySystem;
    private readonly ShowCrimeRecordsSystem _parentSystem;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public EntityCrimeRecordsOverlay(
        IEntityManager entManager,
        InventorySystem inventorySystem,
        ShowCrimeRecordsSystem showCrimSystem)
    {
        _entManager = entManager;
        _inventorySystem = inventorySystem;
        _parentSystem = showCrimSystem;
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
        // da pizda
        this.ZIndex = this.ZIndex -= 1;

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

            if (GetRecord(hum.Owner, args.MapId, out var criminalType))
            {
                var icon = criminalType switch
                {
                    EnumCriminalRecordType.Released => "released",
                    EnumCriminalRecordType.Discharged => "discharged",
                    EnumCriminalRecordType.Parolled => "parolled",
                    EnumCriminalRecordType.Suspected => "suspected",
                    EnumCriminalRecordType.Wanted => "wanted",
                    EnumCriminalRecordType.Incarcerated => "incarcerated",
                    _ => "released"
                };

                var sprite_icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/White/Interface/records.rsi"), icon);
                var _iconTexture = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(sprite_icon);

                float yOffset;
                float xOffset;
                if (spriteQuery.TryGetComponent(hum.Owner, out var sprite))
                {
                    yOffset = sprite.Bounds.Height + 7f - 7f; //sprite.Bounds.Height + 7f;
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
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
    }

    private bool GetRecord(EntityUid uid, MapId mapId, out EnumCriminalRecordType type)
    {
        if (!_entManager.TryGetComponent(uid, out MetaDataComponent? meta))
        {
            type = EnumCriminalRecordType.Released;
            return false;
        }

        var serverList = _entManager.EntityQuery<CriminalRecordsServerComponent>();
        foreach (var server in serverList)
        {
            // if all good - check avaible records
            foreach (var (key, info) in server.Cache)
            {
                // Check id
                if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
                {
                    // PDA
                    if (_entManager.TryGetComponent(idUid, out PdaComponent? pda) && _entManager.TryGetComponent<IdCardComponent>(pda.ContainedId, out var id))
                    {
                        var idCard = id;
                        if (idCard.FullName == info.StationRecord.Name &&
                            idCard.JobTitle == info.StationRecord.JobTitle)
                        {
                            type = info.CriminalType;
                            return true;
                        }
                    }
                    // ID Card
                    if (_entManager.TryGetComponent(idUid, out id))
                    {
                        var idCard = id;
                        if (idCard.FullName == info.StationRecord.Name &&
                            idCard.JobTitle == info.StationRecord.JobTitle)
                        {
                            type = info.CriminalType;
                            return true;
                        }
                    }
                }
                // Check DNA (Dirty Nanotrasen tehnology lol)
                // And yeah, he can't check - is pulled mask or not
                // it's only Content.Server logic, idk hot it impl to Content.Client
                if (_parentSystem.CanIdentityName(uid) != meta.EntityName)
                    continue;
                if (meta.EntityName != info.StationRecord.Name)
                    continue;
                type = info.CriminalType;
                return true;
            }
        }

        type = EnumCriminalRecordType.Released;
        return false;
    }
}
