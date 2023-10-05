using Content.Server.Popups;
using Content.Server.Weapons.Melee.ItemToggle;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Toggleable;
using Robust.Shared.Player;

namespace Content.Server.White.Other.RechargeableSystem;


public sealed class RechargeableSystem : EntitySystem
{

    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RechargeableComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RechargeableComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<RechargeableComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<RechargeableComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, RechargeableComponent component, ExaminedEvent args)
    {
        if (component.Discharged)
        {
            var remainingTime = (int) (component.RechargeDelay - component.AccumulatedFrametime);
            args.PushMarkup("Он [color=red]разряжен[/color].");
            args.PushMarkup($"Осталось времени для зарядки: [color=green]{remainingTime}[/color] секунд.");
            return;
        }

        var currentCharge = (int) (100 * component.Charge / component.MaxCharge);
        args.PushMarkup($"Текущий заряд: [color=green]{currentCharge}%[/color]");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var rechargeable in EntityManager.EntityQuery<RechargeableComponent>())
        {
            if (!rechargeable.Discharged && rechargeable.Charge == rechargeable.MaxCharge)
                continue;

            rechargeable.AccumulatedFrametime += frameTime;

            var delay = rechargeable.Discharged ? rechargeable.RechargeDelay : 1f;

            if (rechargeable.AccumulatedFrametime < delay)
                continue;

            rechargeable.AccumulatedFrametime -= delay;

            if (rechargeable.Discharged)
            {
                rechargeable.Discharged = false;
                rechargeable.Charge = rechargeable.MaxCharge;
            }
            else
            {
                rechargeable.Charge = FixedPoint2.Min(rechargeable.MaxCharge,
                    rechargeable.Charge + rechargeable.ChargePerSecond);
            }
        }
    }

    private void OnUseInHand(EntityUid uid, RechargeableComponent component, UseInHandEvent args)
    {
        if (!component.Discharged)
            return;

        args.Handled = true;

        _audio.Play(_audio.GetSound(component.TurnOnFailSound), Filter.Pvs(uid, entityManager: EntityManager), uid, true,
            AudioHelpers.WithVariation(0.25f));
        _popup.PopupEntity(Loc.GetString("stunbaton-component-low-charge"), args.User);
    }

    private void OnDamageChanged(EntityUid uid, RechargeableComponent component, DamageChangedEvent args)
    {
        if (component.Discharged || args.DamageDelta == null)
            return;

        var totalDamage = args.DamageDelta.Total;

        component.Charge = FixedPoint2.Max(FixedPoint2.Zero, component.Charge - totalDamage);

        if (component.Charge > FixedPoint2.Zero)
            return;

        var ev = new ItemToggleDeactivatedEvent();
        RaiseLocalEvent(uid, ref ev);

        component.Discharged = true;

        _appearance.SetData(uid, ToggleableLightVisuals.Enabled, false);
    }

    private void OnInit(EntityUid uid, RechargeableComponent component, ComponentInit args)
    {
        component.Charge = component.MaxCharge;
    }
}
