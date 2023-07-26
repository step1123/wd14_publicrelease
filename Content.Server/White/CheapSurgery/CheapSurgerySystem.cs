using Content.Server.Body.Systems;
using Content.Server.Construction;
using Content.Server.Construction.Components;
using Content.Server.Kitchen.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.White.CheapSurgery;
using Robust.Shared.Utility;

namespace Content.Server.White.CheapSurgery;

public sealed class CheapSurgerySystem : SharedCheapSurgerySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ConstructionSystem _construction = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyComponent, InteractUsingEvent>(OnUsing);
        SubscribeNetworkEvent<OnOrganSelected>(OnSelected);
    }

    private void OnSelected(OnOrganSelected ev)
    {
        if (TryComp<BodyPartComponent>(ev.Uid, out var partComponent) && partComponent.Body != null)
        {
            StartDrop(partComponent.Body.Value, ev.Uid);
        }
        else if (TryComp<OrganComponent>(ev.Uid, out var organComponent) && organComponent.Body != null)
        {
            StartDrop(organComponent.Body.Value, ev.Uid);
        }
    }


    private void OnUsing(EntityUid uid, BodyComponent component, InteractUsingEvent args)
    {
        if(args.Handled || !TryComp<SharpComponent>(args.Used,out _) || _mobState.IsAlive(uid)
           || TryComp<ActiveSurgeryComponent>(uid,out _))
            return;
        if(!TryComp<HumanoidAppearanceComponent>(uid,out _))
            return;

        var organs = GenList(uid);

        var ev = new OnSurgeryStarted(uid,organs);
        RaiseNetworkEvent(ev,args.User);
    }

    private OrganItem GetOrganItem(EntityUid part,List<OrganItem>? child = null)
    {
        var metadata = MetaData(part);

        var organ = new OrganItem(metadata.EntityName, part,
            new SpriteSpecifier.EntityPrototype(metadata.EntityPrototype!.ID));

        if (child != null)
            organ.Children = child;

        return organ;
    }

    public List<OrganItem> GenList(EntityUid uid)
    {
        var organs = new List<OrganItem>();

        if (TryComp<BodyComponent>(uid, out var bodyComponent))
        {
            foreach (var (part, _) in _body.GetBodyChildren(uid, bodyComponent))
            {
                var child = GenList(part);
                if(child.Count > 0)
                    organs.Add(GetOrganItem(part,child));
            }
        }
        else if (TryComp<BodyPartComponent>(uid, out var partComponent))
        {
            foreach (var (part,_) in _body.GetPartChildren(uid,partComponent))
            {
                var child = GenList(part);
                if(child.Count > 0)
                    organs.Add(GetOrganItem(part,child));
            }

            foreach (var (part,_) in _body.GetPartOrgans(uid,partComponent))
            {
                organs.Add(GetOrganItem(part,GenList(part)));
            }
        }


        return organs;
    }

    public bool StartDrop(EntityUid uid,EntityUid organUid,EntityUid? user = null, BodyComponent? component = null)
    {
        if(!Resolve(uid,ref component))
            return false;

        EnsureComp<ActiveSurgeryComponent>(uid).OrganUid = organUid;

        var construct = EnsureComp<ConstructionComponent>(uid);
        return _construction.ChangeGraph(uid, user, "BodySurgery", "head", true, construct);
    }

}
