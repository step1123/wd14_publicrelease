using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgMagazineFillupSystem : EntitySystem
{
    private const string MagazineSlot = "gun_magazine";
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgAmmoFillupModuleComponent, ModuleInsertEvent>(OnModuleInsert);
        SubscribeLocalEvent<CyborgAmmoFillupModuleComponent, ModuleRemoveEvent>(OnModuleRemove);

        SubscribeLocalEvent<GunComponent, CyborgInstrumentGotInsertedEvent>(OnInserted);
        SubscribeLocalEvent<GunComponent, CyborgInstrumentGotPickupEvent>(OnPickup);
        SubscribeLocalEvent<ActiveAmmoFillerComponent, GunShotEvent>(OnTakeAmmo);
    }

    private void OnModuleRemove(EntityUid uid, CyborgAmmoFillupModuleComponent component, ModuleRemoveEvent args)
    {
        RemComp<CyborgHasFillupAmmoComponent>(args.CyborgUid);
    }

    private void OnModuleInsert(EntityUid uid, CyborgAmmoFillupModuleComponent component, ModuleInsertEvent args)
    {
        EnsureComp<CyborgHasFillupAmmoComponent>(args.CyborgUid);
    }

    private void OnTakeAmmo(EntityUid uid, ActiveAmmoFillerComponent component, ref GunShotEvent args)
    {
        if (!TryComp<CyborgInstrumentComponent>(uid, out var cyborgInstrumentComponent) ||
            !TryComp<CyborgHasFillupAmmoComponent>(cyborgInstrumentComponent.CyborgUid, out _))
            return;

        if (_cyborg.TryChangeEnergy(cyborgInstrumentComponent.CyborgUid, -component.EnergyCost))
            FillMagazine(uid);
    }

    private void OnPickup(EntityUid uid, GunComponent component, CyborgInstrumentGotPickupEvent args)
    {
        EnsureComp<ActiveAmmoFillerComponent>(uid).NextUpdateTime = _timing.CurTime;
    }

    private void OnInserted(EntityUid uid, GunComponent component, CyborgInstrumentGotInsertedEvent args)
    {
        RemComp<ActiveAmmoFillerComponent>(uid);
    }

    private bool TryGetMagazineEntity(EntityUid uid, out EntityUid? magazineUid)
    {
        magazineUid = null;
        if (!_container.TryGetContainer(uid, MagazineSlot, out var container) ||
            container is not ContainerSlot slot)
            return false;

        magazineUid = slot.ContainedEntity;
        return true;
    }


    public void FillMagazine(EntityUid uid)
    {
        if (TryGetMagazineEntity(uid, out var magazineUid) &&
            TryComp<BallisticAmmoProviderComponent>(magazineUid, out var ballisticAmmoProvider))
            ballisticAmmoProvider.UnspawnedCount += 1;
    }
}
