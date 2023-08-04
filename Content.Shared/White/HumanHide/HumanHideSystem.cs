using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;

namespace Content.Shared.White.HumanHide;

public sealed class HumanHideSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HumanHideComponent,GotEquippedEvent>(OnStartup);
        SubscribeLocalEvent<HumanHideComponent,GotUnequippedEvent>(OnShutdown);
    }

    private void OnShutdown(EntityUid uid, HumanHideComponent component, GotUnequippedEvent args)
    {
        foreach (var layer in (HumanoidVisualLayers[]) Enum.GetValues(typeof(HumanoidVisualLayers)))
        {
            _humanoid.SetLayerVisibility(args.Equipee,layer,true,true);
        }
    }

    private void OnStartup(EntityUid uid, HumanHideComponent component, GotEquippedEvent args)
    {
        foreach (var layer in (HumanoidVisualLayers[]) Enum.GetValues(typeof(HumanoidVisualLayers)))
        {
            _humanoid.SetLayerVisibility(args.Equipee,layer,false,true);
        }
    }


}
