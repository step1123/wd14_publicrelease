using Content.Server.Body.Components;
using Content.Server.Chemistry.ReagentEffects;
using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Content.Shared.White.Cyborg.Systems;

namespace Content.Server.White.Cyborg.Systems;

public sealed class MMISystem : SharedMMISystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MMIComponent,InteractUsingEvent>(OnInteract);
        SubscribeLocalEvent<CyborgBrainComponent,BrainInsertedToMMIEvent>(OnInserted);
        SubscribeLocalEvent<MMIComponent,MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<MMIComponent,MindRemovedMessage>(OnMindRemoved);
    }

    private void OnMindRemoved(EntityUid uid, MMIComponent component, MindRemovedMessage args)
    {
        component.IsActive = false;
        RemComp<ActiveCyborgBrainComponent>(uid);
        UpdateAppearance(uid,component);
    }

    private void OnMindAdded(EntityUid uid, MMIComponent component, MindAddedMessage args)
    {
        component.IsActive = true;
        EnsureComp<ActiveCyborgBrainComponent>(uid);
        UpdateAppearance(uid,component);
    }

    private void OnInserted(EntityUid uid, CyborgBrainComponent component, BrainInsertedToMMIEvent args)
    {
        if(!TryComp<MindContainerComponent>(args.BrainUid,out var mind))
        {
            args.Cancel();
            return;
        }

        if(_mind.TryGetMind(args.BrainUid,out var brainMind))
            _mind.TransferTo(brainMind,uid);
        else
            args.Cancel();
    }

    private void OnInteract(EntityUid uid, MMIComponent component, InteractUsingEvent args)
    {
        if(!TryComp<BrainComponent>(args.Used,out var brainComponent))
            return;

        InsertBrain(uid,args.Used,component,brainComponent);
    }

    public void InsertBrain(EntityUid uid, EntityUid brainUid, MMIComponent? component = null,
        BrainComponent? brainComponent = null)
    {
        if(!Resolve(uid,ref component) || !Resolve(brainUid,ref brainComponent))
            return;

        var ev = new BrainInsertedToMMIEvent(brainUid);
        RaiseLocalEvent(uid,ev);

        if(ev.Cancelled)
        {
            _popup.PopupEntity("Похоже у мозга нет нейронных активностей",uid);
            return;
        }

        component.BrainContainer.Insert(brainUid);

        UpdateAppearance(uid,component);
    }

    public void UpdateAppearance(EntityUid uid, MMIComponent? component = null, AppearanceComponent? appearanceComponent = null)
    {
        if(!Resolve(uid,ref component,ref appearanceComponent))
            return;

        _appearance.SetData(uid,MMIVisuals.Status, component.IsActive ? MMIStatus.On : MMIStatus.Off );
        _appearance.SetData(uid,MMIVisuals.HasBrain,component.BrainContainer.ContainedEntity.HasValue );
    }
}
