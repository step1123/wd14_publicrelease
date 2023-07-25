using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Robust.Server.Containers;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgBrainSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgBrainComponent,BrainGotInsertedToBorgEvent>(OnInserted);
        SubscribeLocalEvent<CyborgBrainComponent,BrainGotRemovedFromBorgEvent>(OnRemoved);
        SubscribeLocalEvent<CyborgBrainComponent,CyborgPartGetGibbedEvent>(OnGibbed);
    }

    private void OnGibbed(EntityUid uid, CyborgBrainComponent component, CyborgPartGetGibbedEvent args)
    {
        var ev = new BrainGotRemovedFromBorgEvent(args.CyborgUid);
        RaiseLocalEvent(uid, ev);
    }

    private void OnRemoved(EntityUid uid, CyborgBrainComponent component, BrainGotRemovedFromBorgEvent args)
    {
        component.CyborgUid = null;

        if (TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent))
            cyborgComponent.BrainUid = null;

        if (TryComp<MindContainerComponent>(args.CyborgUid, out var mindContainer))
        {
            if (_mind.TryGetMind(args.CyborgUid, out var mind, mindContainer))
                _mind.TransferTo(mind, uid);
        }

    }

    private void OnInserted(EntityUid uid, CyborgBrainComponent component, BrainGotInsertedToBorgEvent args)
    {
        component.CyborgUid = args.CyborgUid;

        if (TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent))
            cyborgComponent.BrainUid = uid;


        if (!TryComp<MindContainerComponent>(uid,out var mindContainer) || !mindContainer.HasMind)
            return;
        if (!_mind.TryGetMind(uid, out var mind))
            return;
        _mind.TransferTo(mind, args.CyborgUid);
    }

    public void InsertBrain(EntityUid uid, EntityUid brainUid, CyborgComponent? component = null)
    {
        if(!Resolve(uid,ref component))
            return;

        if(!component.BrainSlot.Insert(brainUid))
            return;

        var ev = new BrainGotInsertedToBorgEvent(uid);
        RaiseLocalEvent(brainUid,ev);
    }

    public void RemoveBrain(EntityUid uid, CyborgComponent? component = null)
    {
        if(!Resolve(uid,ref component))
            return;

        var brainUid = component.BrainSlot.ContainedEntity;

        _container.EmptyContainer(component.BrainSlot);

        var ev = new BrainGotRemovedFromBorgEvent(uid);
        if (brainUid.HasValue)
            RaiseLocalEvent(brainUid.Value, ev);
    }
}
