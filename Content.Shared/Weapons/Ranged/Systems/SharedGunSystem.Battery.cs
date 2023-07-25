using Content.Shared.Examine;
using Content.Shared.Item;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;

    protected virtual void InitializeBattery()
    {
        // Trying to dump comp references hence the below
        // Hitscan
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ComponentGetState>(OnBatteryGetState);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ComponentHandleState>(OnBatteryHandleState);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, TakeAmmoEvent>(OnBatteryTakeAmmo);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, GetAmmoCountEvent>(OnBatteryAmmoCount);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ExaminedEvent>(OnBatteryExamine);

        // Projectile
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ComponentGetState>(OnBatteryGetState);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ComponentHandleState>(OnBatteryHandleState);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, TakeAmmoEvent>(OnBatteryTakeAmmo);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, GetAmmoCountEvent>(OnBatteryAmmoCount);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ExaminedEvent>(OnBatteryExamine);

        // TwoModeEnergy
        SubscribeLocalEvent<TwoModeEnergyAmmoProviderComponent, ComponentInit>(OnTwoModeInit);
        SubscribeLocalEvent<TwoModeEnergyAmmoProviderComponent, ComponentGetState>(OnBatteryTwoModeGetState);
        SubscribeLocalEvent<TwoModeEnergyAmmoProviderComponent, ComponentHandleState>(OnBatteryTwoModeHandleState);
        SubscribeLocalEvent<TwoModeEnergyAmmoProviderComponent, TakeAmmoEvent>(OnBatteryTakeAmmo);
        SubscribeLocalEvent<TwoModeEnergyAmmoProviderComponent, GetAmmoCountEvent>(OnBatteryAmmoCount);
        SubscribeLocalEvent<TwoModeEnergyAmmoProviderComponent, ExaminedEvent>(OnBatteryExamine);
    }


    private void OnTwoModeInit(EntityUid uid, TwoModeEnergyAmmoProviderComponent component, ComponentInit args)
    {
        if (!Timing.IsFirstTimePredicted || !TryComp<AppearanceComponent>(component.Owner, out var appearance)) return;

        Appearance.SetData(appearance.Owner, AmmoVisuals.InStun, component.InStun, appearance);
    }

    private void OnBatteryTwoModeHandleState(EntityUid uid, TwoModeEnergyAmmoProviderComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not TwoModeComponentState state)
            return;

        component.Shots = state.Shots;
        component.Capacity = state.MaxShots;
        component.FireCost = state.FireCost;
        component.CurrentMode = state.CurrentMode;
        component.InStun = state.InStun;
    }

    private void OnBatteryTwoModeGetState(EntityUid uid, TwoModeEnergyAmmoProviderComponent component, ref ComponentGetState args)
    {
        args.State = new TwoModeComponentState()
        {
            Shots = component.Shots,
            MaxShots = component.Capacity,
            FireCost = component.FireCost,
            CurrentMode = component.CurrentMode,
            InStun = component.InStun
        };
    }

    protected void UpdateTwoModeAppearance(EntityUid uid, TwoModeEnergyAmmoProviderComponent component)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;
        if (!TryComp<ItemComponent?>(uid, out var item))
            return;

        if (component.InStun)
            _item.SetHeldPrefix(uid, null, item);
        else
            _item.SetHeldPrefix(uid, "laser", item);


        Appearance.SetData(uid, AmmoVisuals.InStun, component.InStun, appearance);
    }

    private void OnBatteryHandleState(EntityUid uid, BatteryAmmoProviderComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not BatteryAmmoProviderComponentState state)
            return;

        component.Shots = state.Shots;
        component.Capacity = state.MaxShots;
        component.FireCost = state.FireCost;
    }

    private void OnBatteryGetState(EntityUid uid, BatteryAmmoProviderComponent component, ref ComponentGetState args)
    {
        args.State = new BatteryAmmoProviderComponentState()
        {
            Shots = component.Shots,
            MaxShots = component.Capacity,
            FireCost = component.FireCost,
        };
    }

    private void OnBatteryExamine(EntityUid uid, BatteryAmmoProviderComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("gun-battery-examine", ("color", AmmoExamineColor), ("count", component.Shots)));
    }

    private void OnBatteryTakeAmmo(EntityUid uid, BatteryAmmoProviderComponent component, TakeAmmoEvent args)
    {
        var shots = Math.Min(args.Shots, component.Shots);

        // Don't dirty if it's an empty fire.
        if (shots == 0)
            return;

        for (var i = 0; i < shots; i++)
        {
            args.Ammo.Add(GetShootable(component, args.Coordinates));
            component.Shots--;
        }

        TakeCharge(uid, component);
        UpdateBatteryAppearance(uid, component);
        Dirty(component);
    }

    private void OnBatteryAmmoCount(EntityUid uid, BatteryAmmoProviderComponent component, ref GetAmmoCountEvent args)
    {
        args.Count = component.Shots;
        args.Capacity = component.Capacity;
    }

    /// <summary>
    /// Update the battery (server-only) whenever fired.
    /// </summary>
    protected virtual void TakeCharge(EntityUid uid, BatteryAmmoProviderComponent component) {}

    protected void UpdateBatteryAppearance(EntityUid uid, BatteryAmmoProviderComponent component)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        Appearance.SetData(uid, AmmoVisuals.HasAmmo, component.Shots != 0, appearance);
        Appearance.SetData(uid, AmmoVisuals.AmmoCount, component.Shots, appearance);
        Appearance.SetData(uid, AmmoVisuals.AmmoMax, component.Capacity, appearance);
    }

    private (EntityUid? Entity, IShootable) GetShootable(BatteryAmmoProviderComponent component, EntityCoordinates coordinates)
    {
        switch (component)
        {
            case ProjectileBatteryAmmoProviderComponent proj:
                var ent = Spawn(proj.Prototype, coordinates);
                return (ent, EnsureComp<AmmoComponent>(ent));
            case HitscanBatteryAmmoProviderComponent hitscan:
                return (null, ProtoManager.Index<HitscanPrototype>(hitscan.Prototype));
            case TwoModeEnergyAmmoProviderComponent twoMode:
                if (twoMode.CurrentMode == EnergyModes.Stun)
                {
                    var projEnt = Spawn(twoMode.ProjectilePrototype, coordinates);
                    return (projEnt, EnsureComp<AmmoComponent>(projEnt));
                }
                var projEnt2 = Spawn(twoMode.HitscanPrototype, coordinates);
                return (projEnt2, EnsureComp<AmmoComponent>(projEnt2));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Serializable, NetSerializable]
    private sealed class BatteryAmmoProviderComponentState : ComponentState
    {
        public int Shots;
        public int MaxShots;
        public float FireCost;
    }

    [Serializable, NetSerializable]
    public sealed class TwoModeComponentState : ComponentState
    {
        public EnergyModes CurrentMode { get; init; }
        public int Shots;
        public int MaxShots;
        public float FireCost;
        public bool InStun;
    }
}
