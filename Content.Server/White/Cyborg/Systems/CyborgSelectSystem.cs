using Content.Server.Polymorph.Systems;
using Content.Server.White.Radials;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.Components;
using Robust.Server.Containers;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cyborg.Systems;


public sealed class CyborgSelectSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly CyborgBrainSystem _cyborgBrain = default!;
    [Dependency] private readonly CyborgSystem _cyborg = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SelectCyborgComponent,CyborgSelectedMessage>(OnCyborgSelected);
        SubscribeLocalEvent<SelectCyborgComponent, PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(EntityUid uid, SelectCyborgComponent component, PlayerAttachedEvent args)
    {
        if (!_uiSystem.TryGetUi(uid, CyborgSelectUiKey.Key, out var ui))
            return;

        _uiSystem.TryOpen(uid, CyborgSelectUiKey.Key, args.Player);
    }

    private void OnCyborgSelected(EntityUid uid, SelectCyborgComponent component, CyborgSelectedMessage args)
    {
        if(!TryComp<CyborgComponent>(uid,out var cyborgComponent))
            return;

        EntityUid? battery = null;
        if (cyborgComponent.BatterySlot.ContainedEntity != null)
        {
            battery = cyborgComponent.BatterySlot.ContainedEntity.Value;
            _cyborg.RemoveBattery(uid,cyborgComponent);
        }

        EntityUid? brain = null;
        if (cyborgComponent.BrainSlot.ContainedEntity != null)
        {
            brain = cyborgComponent.BrainSlot.ContainedEntity.Value;
            _cyborgBrain.RemoveBrain(uid,cyborgComponent);
        }

        var polyUid = _polymorphSystem.PolymorphEntity(uid, args.SelectedPolyMorph);
        if(polyUid == null)
            return;

        if (battery.HasValue)
            _cyborg.InsertBattery(polyUid.Value,battery.Value);
        if (brain.HasValue)
            _cyborgBrain.InsertBrain(polyUid.Value,brain.Value);
    }

}
