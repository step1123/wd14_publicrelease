using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.White.Cyborg.Components;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Systems;

public abstract class SharedMMISystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<MMIComponent, UseAttemptEvent>(OnCancel);
        SubscribeLocalEvent<MMIComponent, InteractionAttemptEvent>(OnCancel);
        SubscribeLocalEvent<MMIComponent, DropAttemptEvent>(OnCancel);
        SubscribeLocalEvent<MMIComponent, PickupAttemptEvent>(OnCancel);
        SubscribeLocalEvent<MMIComponent, UpdateCanMoveEvent>(OnCancel);
        SubscribeLocalEvent<MMIComponent, ChangeDirectionAttemptEvent>(OnCancel);

        SubscribeLocalEvent<MMIComponent,ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, MMIComponent component, ComponentStartup args)
    {
        component.BrainContainer = _container.EnsureContainer<ContainerSlot>(uid, MMIComponent.BrainContainerName);
    }

    private void OnCancel(EntityUid uid, MMIComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    [Serializable, NetSerializable]
    public enum MMIVisuals : byte
    {
        Status,
        HasBrain
    }

    [Serializable, NetSerializable]
    public enum MMIStatus : byte
    {
        Off,
        On,
        Dead
    }

}
