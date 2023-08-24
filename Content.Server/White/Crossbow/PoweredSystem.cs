using System.Diagnostics.CodeAnalysis;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Server.White.Crossbow;

public sealed class PoweredSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    private const string CellSlot = "cell_slot";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PoweredComponent, AmmoShotEvent>(OnShoot);
    }

    private void OnShoot(EntityUid uid, PoweredComponent component, AmmoShotEvent args)
    {
        if (!TryGetBatteryComponent(uid, out var battery, out var batteryUid))
            return;

        var (factor, charge) = GetFactor(component, battery, batteryUid.Value);

         // Чтобы затриггерить взрыв на полную мощь, если есть плазма в батарейке
        _battery.SetCharge(batteryUid.Value, battery.CurrentCharge, battery);

        _battery.SetCharge(batteryUid.Value, battery.CurrentCharge - charge, battery);
        var damage = component.Damage * factor;

        foreach (var proj in args.FiredProjectiles)
        {
            EnsureComp<ThrowDamageModifierComponent>(proj).Damage += damage;

            if (factor == 0)
                continue;

            var embed = EnsureComp<EmbeddableProjectileComponent>(proj);
            embed.Penetrate = true;
            Dirty(embed);
        }
    }

    public float GetPowerCoefficient(EntityUid uid)
    {
        if (!TryComp(uid, out PoweredComponent? component) ||
            !TryGetBatteryComponent(uid, out var battery, out var batteryUid))
            return 1f;

        return 1f + GetFactor(component, battery, batteryUid.Value).Item1;
    }

    private (float, float) GetFactor(PoweredComponent component, BatteryComponent battery, EntityUid batteryUid)
    {
        DebugTools.Assert(component.Charge != 0f);
        var charge = MathF.Min(battery.CurrentCharge, component.Charge);
        var factor = charge / component.Charge;

        if (TryComp(batteryUid, out RiggableComponent? rig) && rig.IsRigged)
            factor *= 2f;

        return (factor, charge);
    }

    private bool TryGetBatteryComponent(EntityUid uid, [NotNullWhen(true)] out BatteryComponent? battery,
        [NotNullWhen(true)] out EntityUid? batteryUid)
    {
        if (!_containers.TryGetContainer(uid, CellSlot, out var container) ||
            container is not ContainerSlot slot)
        {
            battery = null;
            batteryUid = null;
            return false;
        }

        batteryUid = slot.ContainedEntity;

        if (batteryUid != null)
            return TryComp(batteryUid, out battery);

        battery = null;
        return false;
    }
}
