using Content.Server.Mind.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.White.Cult.Runes.Components;
using Content.Shared.White.Cult.UI;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem
{
    [Dependency] private readonly ItemSlotsSystem _slotsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;


    public void InitializeConstructs()
    {
        SubscribeLocalEvent<ConstructShellComponent, ContainerIsInsertingAttemptEvent>(OnShardInsertAttempt);
        SubscribeLocalEvent<ConstructShellComponent, ComponentInit>(OnShellInit);
        SubscribeLocalEvent<ConstructShellComponent, ComponentRemove>(OnShellRemove);
        SubscribeLocalEvent<ConstructShellComponent, ConstructFormSelectedEvent>(OnShellSelected);
    }

    private void OnShellSelected(EntityUid uid, ConstructShellComponent component, ConstructFormSelectedEvent args)
    {
        var construct = Spawn(args.SelectedForm, Transform(args.Entity).Coordinates);
        var mind = Comp<MindContainerComponent>(args.Session.AttachedEntity!.Value);

        _mindSystem.TransferTo(mind.Mind!, construct);
        Del(args.Entity);
    }

    private void OnShellInit(EntityUid uid, ConstructShellComponent component, ComponentInit args)
    {
       _slotsSystem.AddItemSlot(uid, component.ShardSlotId, component.ShardSlot);
    }

    private void OnShellRemove(EntityUid uid, ConstructShellComponent component, ComponentRemove args)
    {
        _slotsSystem.RemoveItemSlot(uid, component.ShardSlot);
    }

    private void OnShardInsertAttempt(EntityUid uid, ConstructShellComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (!TryComp<MindContainerComponent>(args.EntityUid, out var mindComponent) || !mindComponent.HasMind || mindComponent.Mind.Session == null)
        {
            _popupSystem.PopupEntity("Нет души", uid);
            args.Cancel();
            return;
        }

        _slotsSystem.SetLock(uid, component.ShardSlotId, true);
        _ui.TryOpen(uid, SelectConstructUi.Key, mindComponent.Mind.Session);
    }
}
