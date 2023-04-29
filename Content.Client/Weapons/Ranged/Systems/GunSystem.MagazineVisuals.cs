using Content.Client.Weapons.Ranged.Components;
using Content.Shared.Rounding;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    private void InitializeMagazineVisuals()
    {
        SubscribeLocalEvent<MagazineVisualsComponent, ComponentInit>(OnMagazineVisualsInit);
        SubscribeLocalEvent<MagazineVisualsComponent, AppearanceChangeEvent>(OnMagazineVisualsChange);
    }

    private void OnMagazineVisualsInit(EntityUid uid, MagazineVisualsComponent component, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)) return;

        if (sprite.LayerMapTryGet(GunVisualLayers.Mag, out _))
        {
            sprite.LayerSetState(GunVisualLayers.Mag, $"{component.MagState}-{component.MagSteps - 1}");
            sprite.LayerSetVisible(GunVisualLayers.Mag, false);
        }

        if (sprite.LayerMapTryGet(GunVisualLayers.MagUnshaded, out _))
        {
            sprite.LayerSetState(GunVisualLayers.MagUnshaded, $"{component.MagState}-unshaded-{component.MagSteps - 1}");
            sprite.LayerSetVisible(GunVisualLayers.MagUnshaded, false);
        }

        if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeFirst, out _))
        {
            sprite.LayerSetState(GunVisualLayers.TwoModeFirst, $"{component.MagState}-twomode1-{component.MagSteps - 1}");
            sprite.LayerSetVisible(GunVisualLayers.TwoModeFirst, false);
        }

        if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeSecond, out _))
        {
            sprite.LayerSetState(GunVisualLayers.TwoModeSecond, $"{component.MagState}-twomode2-{component.MagSteps - 1}");
            sprite.LayerSetVisible(GunVisualLayers.TwoModeSecond, false);
        }
    }

    private void OnMagazineVisualsChange(EntityUid uid, MagazineVisualsComponent component, ref AppearanceChangeEvent args)
    {
        // tl;dr
        // 1.If no mag then hide it OR
        // 2. If step 0 isn't visible then hide it (mag or unshaded)
        // 3. Otherwise just do mag / unshaded as is
        var sprite = args.Sprite;

        if (sprite == null) return;

        if (!args.AppearanceData.TryGetValue(AmmoVisuals.MagLoaded, out var magloaded) ||
            magloaded is true)
        {
            if (!args.AppearanceData.TryGetValue(AmmoVisuals.AmmoMax, out var capacity))
            {
                capacity = component.MagSteps;
            }

            if (!args.AppearanceData.TryGetValue(AmmoVisuals.AmmoCount, out var current))
            {
                current = component.MagSteps;
            }

            var step = ContentHelpers.RoundToLevels((int) current, (int) capacity, component.MagSteps);

            if (step == 0 && !component.ZeroVisible)
            {
                if (sprite.LayerMapTryGet(GunVisualLayers.Mag, out _))
                {
                    sprite.LayerSetVisible(GunVisualLayers.Mag, false);
                }

                if (sprite.LayerMapTryGet(GunVisualLayers.MagUnshaded, out _))
                {
                    sprite.LayerSetVisible(GunVisualLayers.MagUnshaded, false);
                }

                if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeFirst, out _))
                {
                    sprite.LayerSetVisible(GunVisualLayers.TwoModeFirst, false);
                }

                if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeSecond, out _))
                {
                    sprite.LayerSetVisible(GunVisualLayers.TwoModeSecond, false);
                }

                return;
            }

            if (sprite.LayerMapTryGet(GunVisualLayers.Mag, out _))
            {
                sprite.LayerSetVisible(GunVisualLayers.Mag, true);
                sprite.LayerSetState(GunVisualLayers.Mag, $"{component.MagState}-{step}");
            }

            if (sprite.LayerMapTryGet(GunVisualLayers.MagUnshaded, out _))
            {
                sprite.LayerSetVisible(GunVisualLayers.MagUnshaded, true);
                sprite.LayerSetState(GunVisualLayers.MagUnshaded, $"{component.MagState}-unshaded-{step}");
            }

            if (!args.AppearanceData.TryGetValue(AmmoVisuals.InStun, out var inStun) || inStun is true)
            {
                if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeFirst, out var _))
                {
                    sprite.LayerSetVisible(GunVisualLayers.TwoModeSecond, false);
                    sprite.LayerSetVisible(GunVisualLayers.TwoModeFirst, true);
                    sprite.LayerSetState(GunVisualLayers.TwoModeFirst, $"{component.MagState}-twomode1-{step}");
                }
            }

            else if (inStun is false)
            {
                if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeSecond, out var _))
                {
                    sprite.LayerSetVisible(GunVisualLayers.TwoModeFirst, false);
                    sprite.LayerSetVisible(GunVisualLayers.TwoModeSecond, true);
                    sprite.LayerSetState(GunVisualLayers.TwoModeSecond, $"{component.MagState}-twomode2-{step}");

                }
            }
        }
        else
        {
            if (sprite.LayerMapTryGet(GunVisualLayers.Mag, out _))
            {
                sprite.LayerSetVisible(GunVisualLayers.Mag, false);
            }

            if (sprite.LayerMapTryGet(GunVisualLayers.MagUnshaded, out _))
            {
                sprite.LayerSetVisible(GunVisualLayers.MagUnshaded, false);
            }

            if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeFirst, out _))
            {
                sprite.LayerSetVisible(GunVisualLayers.TwoModeFirst, false);
            }

            if (sprite.LayerMapTryGet(GunVisualLayers.TwoModeSecond, out _))
            {
                sprite.LayerSetVisible(GunVisualLayers.TwoModeSecond, false);
            }
        }
    }
}
