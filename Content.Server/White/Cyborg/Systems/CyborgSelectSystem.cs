using Content.Server.White.Cyborg.SiliconBrain;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.Components;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgSelectSystem : EntitySystem
{
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SiliconBrainSystem _siliconBrain = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SelectCyborgComponent, CyborgSelectedMessage>(OnCyborgSelected);
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
        ChangeCyborgType(uid, args.SelectedPrototype);
    }

    public EntityUid? ChangeCyborgType(EntityUid uid, string prototype, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return null;


        EntityUid? battery = null;
        if (component.BatterySlot.ContainedEntity != null)
        {
            battery = component.BatterySlot.ContainedEntity.Value;
            _cyborg.RemoveBattery(uid, component);
        }

        EntityUid? brain;
        if (_siliconBrain.TryGetBrain(uid, out brain))
            _siliconBrain.RemoveBrain(uid);

        var parentTransform = Transform(uid);
        var polyUid = Spawn(prototype, parentTransform.Coordinates);
        var polyTransform = Transform(polyUid);

        _metaData.SetEntityName(polyUid, MetaData(uid).EntityName);
        polyTransform.LocalRotation = parentTransform.LocalRotation;

        QueueDel(uid);

        if (battery.HasValue)
            _cyborg.InsertBattery(polyUid, battery.Value);
        if (brain.HasValue)
            _siliconBrain.InsertBrain(polyUid, brain.Value);

        return polyUid;
    }
}
