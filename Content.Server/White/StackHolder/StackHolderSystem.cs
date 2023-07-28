using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Stacks;
using Content.Shared.White.StackHolder;

namespace Content.Server.White.StackHolder;

public sealed class StackHolderSystem : SharedStackHolderSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StackHolderComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<StackHolderComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<StackHolderListComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, StackHolderListComponent component, UseInHandEvent args)
    {
        if (args.Handled || !TryComp<StackHolderComponent>(uid, out var stackHolderComponent))
            return;

        var currentId = component.StackSlots.IndexOf(stackHolderComponent.CurrentStackSlot);
        stackHolderComponent.CurrentStackSlot = component.StackSlots[(currentId + 1) % component.StackSlots.Count];

        var item = _itemSlotsSystem.GetItemOrNull(uid, stackHolderComponent.CurrentStackSlot);
        if (item != null)
            RaiseNetworkEvent(new StackChangeEvent(uid, item.Value), args.User);
    }

    private void OnExamined(EntityUid uid, StackHolderComponent component, ExaminedEvent args)
    {
        var item = _itemSlotsSystem.GetItemOrNull(uid, component.CurrentStackSlot);

        if (item == null)
        {
            args.PushMarkup(Loc.GetString("stack-holder-empty"));
            return;
        }

        if (TryComp<StackComponent>(item, out var stack))
            args.PushMarkup(Loc.GetString("stack-holder", ("number", stack.Count), ("item", item)));
    }

    private void OnAfterInteract(EntityUid uid, StackHolderComponent component, AfterInteractEvent args)
    {
        var item = _itemSlotsSystem.GetItemOrNull(uid, component.CurrentStackSlot);
        if (item == null)
        {
            if (args.Target != null)
                _itemSlotsSystem.TryInsert(uid, component.CurrentStackSlot, args.Target.Value, args.User);
            return;
        }

        var afterEv =
            new AfterInteractEvent(args.User, (EntityUid) item, args.Target, args.ClickLocation, args.CanReach);
        RaiseLocalEvent((EntityUid) item, afterEv);
        if (args.Target != null)
        {
            var ev = new InteractUsingEvent(args.User, (EntityUid) item, args.Target.Value, args.ClickLocation);
            RaiseLocalEvent(args.Target.Value, ev);
        }
    }
}
