using Content.Shared.Tag;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.White.Cyborg.Systems;

public sealed class ModuleComponentGrantingSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ModuleComponentGrantingComponent,ModuleRemoveEvent>(OnRemove);
        SubscribeLocalEvent<ModuleComponentGrantingComponent,ModuleInsertEvent>(OnInsert);
    }

    private void OnInsert(EntityUid uid, ModuleComponentGrantingComponent component, ModuleInsertEvent args)
    {
        foreach (var (name, data) in component.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);

            if (HasComp(args.CyborgUid, newComp.GetType()))
                RemComp(args.CyborgUid, newComp.GetType());

            newComp.Owner = args.CyborgUid;

            var temp = (object) newComp;
            _serializationManager.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(args.CyborgUid, (Component)temp!);

            component.IsActive = true;
        }
    }

    private void OnRemove(EntityUid uid, ModuleComponentGrantingComponent component, ModuleRemoveEvent args)
    {
        if (!component.IsActive) return;

        foreach (var (name, data) in component.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);

            RemComp(args.CyborgUid, newComp.GetType());
        }

        component.IsActive = false;
    }
}
