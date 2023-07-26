using Content.Shared.White.Cyborg.SiliconBrain.Components;
using Robust.Shared.Containers;

namespace Content.Shared.White.Cyborg.SiliconBrain.Systems;

public abstract class SharedSiliconBrainSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconBrainContainerComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<SiliconBrainContainerComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<SiliconBrainContainerComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<SiliconBrainContainerComponent, EntRemovedFromContainerMessage>(OnRemove);
        SubscribeLocalEvent<SiliconBrainContainerComponent, ContainerIsInsertingAttemptEvent>(OnAttemptInsert);
    }

    private void OnMapInit(EntityUid uid, SiliconBrainContainerComponent component, MapInitEvent args)
    {
        if (component.BrainSlot.ContainedEntity.HasValue)
        {
            var ev = new BrainInsertEvent(uid);
            RaiseLocalEvent(component.BrainSlot.ContainedEntity.Value, ev);
        }
    }

    private void OnInit(EntityUid uid, SiliconBrainContainerComponent component, ComponentStartup args)
    {
        component.BrainSlot =
            _container.EnsureContainer<ContainerSlot>(uid, SiliconBrainContainerComponent.BrainSlotId);
        Dirty(component);
    }

    private void OnRemove(EntityUid uid, SiliconBrainContainerComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container != component.BrainSlot)
            return;

        var ev = new BrainRemoveEvent();
        RaiseLocalEvent(args.Entity, ev);
    }

    private void OnInsert(EntityUid uid, SiliconBrainContainerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container != component.BrainSlot)
            return;

        var ev = new BrainInsertEvent(uid);
        RaiseLocalEvent(args.Entity, ev);
    }

    private void OnAttemptInsert(EntityUid uid, SiliconBrainContainerComponent component,
        ContainerIsInsertingAttemptEvent args)
    {
        if (args.Container != component.BrainSlot)
            return;

        if (!HasComp<SiliconBrainComponent>(args.EntityUid))
            args.Cancel();
    }
}
