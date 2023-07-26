using System.Linq;
using Content.Shared.Hands.Components;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Robust.Shared.Containers;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgSystemInstrumentModule : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly CyborgHandsSystem _cyborgHands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyborgInstrumentModuleComponent, ModuleInsertEvent>(OnModuleInsert);
        SubscribeLocalEvent<CyborgInstrumentModuleComponent, ModuleRemoveEvent>(OnModuleRemoved);
        SubscribeLocalEvent<CyborgInstrumentModuleComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, CyborgInstrumentModuleComponent component, MapInitEvent args)
    {
        component.InstrumentContainer =
            _container.EnsureContainer<Container>(uid, CyborgInstrumentModuleComponent.InstrumentContainerName);
    }


    private void OnModuleRemoved(EntityUid uid, CyborgInstrumentModuleComponent component, ModuleRemoveEvent args)
    {
        InsertAllInstrumentIntoModule(args.CyborgUid, uid);
    }


    private void OnModuleInsert(EntityUid uid, CyborgInstrumentModuleComponent component, ModuleInsertEvent args)
    {
        if (!TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent))
            return;

        foreach (var entity in component.InstrumentContainer.ContainedEntities.ToList())
        {
            if (cyborgComponent.InstrumentContainer.Insert(entity))
            {
                component.InstrumentUids.Add(entity);
                cyborgComponent.InstrumentUids.Add(entity);
            }
        }

        Dirty(cyborgComponent);
        _cyborg.UpdateUserInterface(args.CyborgUid);
    }

    public void InsertAllInstrumentIntoModule(EntityUid uid, EntityUid moduleUid,
        CyborgComponent? component = null, CyborgInstrumentModuleComponent? instrument = null,
        HandsComponent? hands = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(moduleUid, ref instrument))
            return;

        if (Resolve(uid, ref hands))
        {
            foreach (var hand in hands.Hands.Values)
            {
                if (!hand.HeldEntity.HasValue)
                    continue;
                _cyborgHands.TryInsertInstrument(uid, hand.HeldEntity.Value, component);
            }
        }

        foreach (var entity in instrument.InstrumentUids.ToList())
        {
            InsertInstrumentIntoModule(uid, moduleUid, entity, component, instrument);
        }

        Dirty(component);
        _cyborg.UpdateUserInterface(uid);
    }

    //Суёт обратно инструменты в модуль
    public void InsertInstrumentIntoModule(EntityUid uid, EntityUid moduleUid, EntityUid entity,
        CyborgComponent? cyborgComponent = null, CyborgInstrumentModuleComponent? component = null)
    {
        if (!Resolve(uid, ref cyborgComponent) || !Resolve(moduleUid, ref component))
            return;

        if (component.InstrumentContainer.Insert(entity))
        {
            component.InstrumentUids.Remove(entity);
            cyborgComponent.InstrumentUids.Remove(entity);
        }
    }
}
