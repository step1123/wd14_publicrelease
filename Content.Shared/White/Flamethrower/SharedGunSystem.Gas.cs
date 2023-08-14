using Content.Shared.Examine;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.White.Flamethrower;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    protected virtual void InitializeGas()
    {
        SubscribeLocalEvent<GasAmmoProviderComponent, TakeAmmoEvent>(OnGasTakeAmmo);
        SubscribeLocalEvent<GasAmmoProviderComponent, GetAmmoCountEvent>(OnGasAmmoCount);
        SubscribeLocalEvent<GasAmmoProviderComponent, ExaminedEvent>(OnGasExamine);
    }

    private void OnGasExamine(EntityUid uid, GasAmmoProviderComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("comp-gas-tank-examine", ("pressure", MathF.Round(component.Pressure))));
    }

    private void OnGasAmmoCount(EntityUid uid, GasAmmoProviderComponent component, ref GetAmmoCountEvent args)
    {
        args.Count = component.Shots;
        args.Capacity = component.Capacity;
    }

    private void OnGasTakeAmmo(EntityUid uid, GasAmmoProviderComponent component, TakeAmmoEvent args)
    {
        var shots = Math.Min(args.Shots, component.Shots);

        if (shots == 0)
            return;

        for (var i = 0; i < shots; i++)
        {
            var ent = Spawn(component.Prototype, args.Coordinates);
            InitShot(uid, component, ent);
            args.Ammo.Add((ent, EnsureComp<AmmoComponent>(ent)));
        }
    }

    protected virtual void InitShot(EntityUid uid, GasAmmoProviderComponent component, EntityUid shotUid) {}
}
