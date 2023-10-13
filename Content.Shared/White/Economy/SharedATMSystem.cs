using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;

namespace Content.Shared.White.Economy;

public abstract class SharedATMSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ATMComponent, ComponentInit>(OnATMInit);
        SubscribeLocalEvent<ATMComponent, ComponentRemove>(OnATMRemoved);
    }

    protected virtual void OnATMInit(EntityUid uid, ATMComponent component, ComponentInit args)
    {
        if(component.CardSlot == null) //Нихуя не олвейс фалс, юнит тест ебалнит
            return;

        _itemSlotsSystem.AddItemSlot(uid, component.SlotId, component.CardSlot);
    }

    private void OnATMRemoved(EntityUid uid, ATMComponent component, ComponentRemove args)
    {
        if(component.CardSlot == null) //Нихуя не олвейс фалс, юнит тест ебалнит
            return;

        _itemSlotsSystem.TryEject(uid, component.CardSlot, null!, out _);
        _itemSlotsSystem.RemoveItemSlot(uid, component.CardSlot);
    }
}
