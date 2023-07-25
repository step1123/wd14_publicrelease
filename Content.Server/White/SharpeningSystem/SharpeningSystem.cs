using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Hippie.SharpeningSystem;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Prototypes;

namespace Content.Server.White.SharpeningSystem;

public sealed class SharpeningSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SharpenedComponent, ComponentInit>(OnSharpenedComponentInit);
        SubscribeLocalEvent<SharpenerComponent, AfterInteractEvent>(OnSharping);
        base.Initialize();
    }

    private void OnSharping(EntityUid uid, SharpenerComponent component, AfterInteractEvent args)
    {
        if(!args.Target.HasValue) return;

        if (!TryComp<MeleeWeaponComponent>(args.Target, out var meleeWeaponComponent))
        {
            _popupSystem.PopupEntity("You can't sharpen that", args.Target.Value, args.User);
            return;
        }

        if (!meleeWeaponComponent.Damage.DamageDict.ContainsKey("Slash"))
        {
            _popupSystem.PopupEntity("The weapon must have a cutting edge", args.Target.Value, args.User);
            return;
        }

        if (HasComp<SharpenedComponent>(args.Target.Value))
        {
            _popupSystem.PopupEntity("Weapon already sharpened", args.Target.Value, args.User);
        }

        EnsureComp<SharpenedComponent>(args.Target.Value).DamageModifier = component.DamageModifier;
    }

    private void OnSharpenedComponentInit(EntityUid uid, SharpenedComponent component, ComponentInit args)
    {
        if (!TryComp<MeleeWeaponComponent>(uid, out var meleeWeaponComponent))
        {
            return;
        }

        meleeWeaponComponent?.Damage?.ExclusiveAdd(new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Slash"), component.DamageModifier));
    }
}
