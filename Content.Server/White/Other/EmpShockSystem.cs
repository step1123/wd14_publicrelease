using Content.Server.Electrocution;
using Content.Server.Emp;
using Content.Shared.Inventory;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;

namespace Content.Server.White.Other;

public sealed class EmpShockSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StatusEffectsComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnEmpPulse(EntityUid uid, StatusEffectsComponent component, ref EmpPulseEvent args)
    {
        if (!_inventorySystem.TryGetSlotEntity(uid, "outerClothing", out var suit) ||
            !_tag.HasTag(suit.Value, "Hardsuit"))
            return;

        if (_electrocution.TryDoElectrocution(uid, null, 10, TimeSpan.FromSeconds(3), false, 1F, component, true))
            args.Affected = true;
    }
}
