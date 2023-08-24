using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.White.Crossbow;
using Robust.Client.GameObjects;

namespace Content.Client.White.Crossbow;

public sealed class DrawableSystem : VisualizerSystem<DrawableComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, DrawableComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        var drawn = args.AppearanceData.TryGetValue(DrawableVisuals.Drawn, out var drawnObj) && drawnObj is true;

        var hasAmmo = args.AppearanceData.TryGetValue(AmmoVisuals.AmmoCount, out var ammoCount) && (int) ammoCount > 0;

        var state = drawn ? "drawn" : hasAmmo ? "loaded" : "base";
        args.Sprite.LayerSetState(0, state);
    }
}
