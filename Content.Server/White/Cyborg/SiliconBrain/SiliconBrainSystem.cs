using Content.Server.Body.Components;
using Content.Server.DoAfter;
using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Shared.DoAfter;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.SiliconBrain;
using Content.Shared.White.Cyborg.SiliconBrain.Components;
using Content.Shared.White.Cyborg.SiliconBrain.Systems;
using Robust.Server.Containers;

namespace Content.Server.White.Cyborg.SiliconBrain;

public sealed class SiliconBrainSystem : SharedSiliconBrainSystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconBrainComponent, BrainInsertEvent>(OnInserted);
        SubscribeLocalEvent<SiliconBrainComponent, BrainRemoveEvent>(OnRemoved);
        SubscribeLocalEvent<SiliconBrainComponent, BeingGibbedEvent>(OnGibbed);

        SubscribeLocalEvent<SiliconBrainComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<ActiveSiliconBrainComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<ActiveSiliconBrainComponent, ComponentStartup>(OnActivate);
        SubscribeLocalEvent<SiliconBrainContainerComponent, SiliconMindDoAfterEvent>(OnSiliconMindAdded);
    }

    private void OnSiliconMindAdded(EntityUid uid, SiliconBrainContainerComponent component, SiliconMindDoAfterEvent args)
    {
        if (!TryComp<SiliconBrainComponent>(args.User, out var brainComponent) || !brainComponent.ParentUid.HasValue)
            return;

        if (_mind.TryGetMind(args.User, out var mind))
            _mind.TransferTo(mind, uid);
    }

    private void OnActivate(EntityUid uid, ActiveSiliconBrainComponent component, ComponentStartup args)
    {
        if (!TryComp<SiliconBrainComponent>(uid, out var brainComponent) || !brainComponent.ParentUid.HasValue)
            return;

        var doAfterArgs = new DoAfterArgs(uid, TimeSpan.FromMilliseconds(300), new SiliconMindDoAfterEvent(),
            brainComponent.ParentUid.Value)
        {
            BreakOnHandChange = false,
            RequireCanInteract = false
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnMindRemoved(EntityUid uid, ActiveSiliconBrainComponent component, MindRemovedMessage args)
    {
        RemComp<ActiveSiliconBrainComponent>(uid);
    }

    private void OnMindAdded(EntityUid uid, SiliconBrainComponent component, MindAddedMessage args)
    {
        EnsureComp<ActiveSiliconBrainComponent>(uid);
    }

    private void OnGibbed(EntityUid uid, SiliconBrainComponent component, BeingGibbedEvent args)
    {
        var ev = new BrainRemoveEvent();
        RaiseLocalEvent(uid, ev);
    }

    private void OnRemoved(EntityUid uid, SiliconBrainComponent component, BrainRemoveEvent args)
    {
        if (!component.ParentUid.HasValue)
            return;

        if (TryComp<SiliconBrainContainerComponent>(component.ParentUid, out var containerComponent))
            containerComponent.BrainUid = null;

        if (TryComp<MindContainerComponent>(component.ParentUid, out var mindContainer))
        {
            if (_mind.TryGetMind(component.ParentUid.Value, out var mind, mindContainer))
                _mind.TransferTo(mind, uid);
        }

        component.ParentUid = null;
    }

    private void OnInserted(EntityUid uid, SiliconBrainComponent component, BrainInsertEvent args)
    {
        component.ParentUid = args.ParentUid;

        if (TryComp<SiliconBrainContainerComponent>(args.ParentUid, out var containerComponent))
            containerComponent.BrainUid = uid;


        if (!TryComp<MindContainerComponent>(uid, out var mindContainer) || !mindContainer.HasMind)
            return;
        if (!_mind.TryGetMind(uid, out var mind))
            return;
        _mind.TransferTo(mind, args.ParentUid);
    }

    public void InsertBrain(EntityUid uid, EntityUid brainUid, SiliconBrainContainerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.BrainSlot.Insert(brainUid))
            return;

        var ev = new BrainInsertEvent(uid);
        RaiseLocalEvent(brainUid, ev);
    }

    public void RemoveBrain(EntityUid uid, SiliconBrainContainerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var brainUid = component.BrainSlot.ContainedEntity;

        _container.EmptyContainer(component.BrainSlot);

        var ev = new BrainRemoveEvent();
        if (brainUid.HasValue)
            RaiseLocalEvent(brainUid.Value, ev);
    }

    public bool HasBrain(EntityUid uid, SiliconBrainContainerComponent? component = null)
    {
        return TryGetBrain(uid, out _, component);
    }

    public bool TryGetBrain(EntityUid uid, out EntityUid? brainUid, SiliconBrainContainerComponent? component = null)
    {
        brainUid = null;
        if (!Resolve(uid, ref component))
            return false;

        brainUid = component.BrainSlot.ContainedEntity;
        return brainUid.HasValue;
    }
}
