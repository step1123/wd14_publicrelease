using Content.Server.Body.Components;
using Content.Server.Construction;
using Content.Server.Power.Components;
using Content.Server.White.Cyborg.SiliconBrain;
using Content.Server.White.Cyborg.Systems;
using Content.Shared.Construction;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.SiliconBrain;
using Content.Shared.White.Cyborg.SiliconBrain.Components;
using Content.Shared.White.Cyborg.Systems;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.White.Construction.Completions;

public sealed class BuildBorg : IGraphAction
{
    [DataField("borgPrototype", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BorgPrototype = string.Empty;

    [DataField("batteryContainer")]
    public string BatteryContainer = "battery-container";

    [DataField("brainContainer")]
    public string BrainContainer = "brain-container";

    private ISawmill _sawmill = default!;
    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        _sawmill = Logger.GetSawmill("BorgBuilder");
        var borgSystem = entityManager.EntitySysManager.GetEntitySystem<CyborgSystem>();
        var borgBrainSystem = entityManager.EntitySysManager.GetEntitySystem<SiliconBrainSystem>();

        if (!entityManager.TryGetComponent(uid, out ContainerManagerComponent? containerManager))
        {
            _sawmill.Warning($"Borg construct entity {uid} did not have a container manager! Aborting build mech action.");
            return;
        }

        if(!TryGetEntityFromContainer(uid,entityManager,BatteryContainer,out var cell,out var batteryContainer)
           || !TryGetEntityFromContainer(uid,entityManager,BrainContainer,out var brain,out var brainContainer))
            return;

        if (!entityManager.TryGetComponent<BatteryComponent>(cell, out var batteryComponent))
        {
            _sawmill.Warning($"Borg construct entity {uid} had an invalid entity in container \"{BatteryContainer}\"! Aborting build mech action.");
            return;
        }

        if (!entityManager.TryGetComponent<SiliconBrainComponent>(brain, out var brainComp))
        {
            _sawmill.Warning($"Borg construct entity {uid} had an invalid entity in container \"{BrainContainer}\"! Aborting build mech action.");
            return;
        }

        if (!entityManager.TryGetComponent<SiliconBrainContainerComponent>(uid, out var siliconContainer))
        {
            _sawmill.Warning($"Borg construct entity {uid} had an invalid entity in container \"{BrainContainer}\"! Aborting build mech action.");
            return;
        }

        var transform = entityManager.GetComponent<TransformComponent>(uid);
        var borg = entityManager.SpawnEntity(BorgPrototype, transform.Coordinates);

        if(!entityManager.TryGetComponent<CyborgComponent>(borg, out var cyborgComp))
            return;

        if (cyborgComp.BatterySlot.ContainedEntity == null)
            borgSystem.InsertBattery(borg, cell.Value, cyborgComp, batteryComponent);


        if (siliconContainer.BrainSlot.ContainedEntity == null)
            borgBrainSystem.InsertBrain(borg,brain.Value,siliconContainer);

        var entChangeEv = new ConstructionChangeEntityEvent(borg, uid);
        entityManager.EventBus.RaiseLocalEvent(uid, entChangeEv);
        entityManager.EventBus.RaiseLocalEvent(borg, entChangeEv, broadcast: true);

        entityManager.QueueDeleteEntity(uid);


    }

    private bool TryGetEntityFromContainer(EntityUid uid,IEntityManager entityManager,string containerName,out EntityUid? entityUid,out IContainer? container)
    {
        entityUid = null;
        container = null;

        var containerSystem = entityManager.EntitySysManager.GetEntitySystem<ContainerSystem>();

        if (!entityManager.TryGetComponent(uid, out ContainerManagerComponent? containerManager))
        {
            _sawmill.Warning($"Borg construct entity {uid} did not have a container manager! Aborting build mech action.");
            return false;
        }

        if (!containerSystem.TryGetContainer(uid, containerName, out container, containerManager))
        {
            _sawmill.Warning($"Borg construct entity {uid} did not have the specified '{containerName}' container! Aborting build mech action.");
            return false;
        }

        if (container.ContainedEntities.Count != 1)
        {
            _sawmill.Warning($"Borg construct entity {uid} did not have exactly one item in the specified '{containerName}' container! Aborting build mech action.");
            return false;
        }

        entityUid = container.ContainedEntities[0];
        return true;
    }
}
