using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item;
using Content.Shared.PowerCell.Components;
using Content.Shared.Throwing;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;

namespace Content.Shared.White.Cyborg.Systems;

public abstract class SharedCyborgHandSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgComponent, PickupAttemptEvent>(OnChancel);
        SubscribeLocalEvent<CyborgComponent, ThrowAttemptEvent>(OnChancel);
    }


    private void OnChancel(EntityUid uid, CyborgComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }


    public bool TryPickupInstrument(EntityUid uid, EntityUid entity, CyborgComponent? component = null,
        HandsComponent? handsComponent = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(uid, ref handsComponent))
            return false;

        if (handsComponent.ActiveHand == null ||
            !component.InstrumentUids.Remove(entity))
            return false;

        _hands.DoPickup(uid, handsComponent.ActiveHand, entity, handsComponent);

        var cyborgInstrument = AddComp<CyborgInstrumentComponent>(entity);
        cyborgInstrument.CyborgUid = uid;
        if (TryComp<ItemSlotsComponent>(entity, out var itemSlotsComponent) &&
            TryComp<PowerCellSlotComponent>(entity, out var powerCellSlotComponent))
            cyborgInstrument.BatteryUid = itemSlotsComponent.Slots[powerCellSlotComponent.CellSlotId].Item;

        component.Consumption += component.ModuleConsumption;

        var ev = new CyborgInstrumentGotPickupEvent(uid);
        RaiseLocalEvent(entity, ev);

        return true;
    }


    public bool TryInsertInstrument(EntityUid uid, EntityUid entity, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component) || !RemComp<CyborgInstrumentComponent>(entity)
                                         || !component.InstrumentContainer.Insert(entity))
            return false;
        component.Consumption -= component.ModuleConsumption;
        component.InstrumentUids.Add(entity);

        var ev = new CyborgInstrumentGotInsertedEvent(uid);
        RaiseLocalEvent(entity, ev);

        return true;
    }
}
