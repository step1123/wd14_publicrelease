using System.Linq;
using Content.Server.Construction;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.White.Cyborg.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Components;
using Content.Shared.White.Cyborg.Charge;
using Content.Shared.White.Cyborg.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.Charge;

public sealed class SiliconChargerSystem : SharedSiliconChargerSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CyborgSystem _cyborg = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconChargerComponent, RefreshPartsEvent>(OnRefreshParts);
        SubscribeLocalEvent<SiliconChargerComponent, UpgradeExamineEvent>(OnExamineParts);

        SubscribeLocalEvent<SiliconChargerComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<SiliconChargerComponent, EndCollideEvent>(OnEndCollide);

        SubscribeLocalEvent<SiliconChargerComponent, ComponentShutdown>(OnChargerShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        #region Step Trigger Chargers
        // Check for any chargers with the StepTriggerComponent.
        var stepQuery = EntityQueryEnumerator<SiliconChargerComponent, StepTriggerComponent>();
        while (stepQuery.MoveNext(out var uid, out var chargerComp, out _))
        {
            if (chargerComp.PresentEntities.Count == 0 ||
                TryComp<ApcPowerReceiverComponent>(uid, out var powerComp) && !powerComp.Powered)
            {
                if (chargerComp.Active)
                {
                    chargerComp.Active = false;
                    UpdateState(uid, chargerComp);
                }
                continue;
            }

            if (!chargerComp.Active)
            {
                chargerComp.Active = true;
                UpdateState(uid, chargerComp);
            }

            var chargeRate = frameTime * chargerComp.ChargeMulti / chargerComp.PresentEntities.Count;

            foreach (var entity in chargerComp.PresentEntities.ToList())
            {
                HandleChargingEntity(entity, chargeRate, chargerComp, uid, frameTime);
            }
        }
        #endregion Step Trigger Chargers
    }

    // Cleanup the sound stream when the charger is destroyed.
    private void OnChargerShutdown(EntityUid uid, SiliconChargerComponent component, ComponentShutdown args)
    {
        component.SoundStream?.Stop();
    }

    /// <summary>
    ///     Handles working out what entities need to have their batteries charged, or be burnt.
    /// </summary>
    private void HandleChargingEntity(EntityUid entity, float chargeRate, SiliconChargerComponent chargerComp, EntityUid chargerUid, float frameTime, bool burn = true)
    {
        if (TryComp<CyborgComponent>(entity, out var cyborgComponent))
        {
            _cyborg.TryChangeEnergy(entity, chargeRate, cyborgComponent);
        }
        else if(TryComp<DamageableComponent>(entity,out var damageableComponent))
        {
            BurnEntity(entity,damageableComponent,frameTime,chargerComp, chargerUid);
        }
    }


    private void BurnEntity(EntityUid entity, DamageableComponent damageComp, float frameTime, SiliconChargerComponent chargerComp, EntityUid chargerUid)
    {
        var damage = new DamageSpecifier(_prototypes.Index<DamageTypePrototype>(chargerComp.DamageType), frameTime * chargerComp.ChargeMulti / 100);
        var damageDealt = _damageable.TryChangeDamage(entity, damage, false, true, damageComp, chargerUid);

        if (damageDealt != null && damageDealt.Total > 0 && chargerComp.WarningTime < _timing.CurTime)
        {
            var popupBurn = Loc.GetString(chargerComp.OverheatString);
            _popup.PopupEntity(popupBurn, entity, entity, PopupType.MediumCaution);

            chargerComp.WarningTime = TimeSpan.FromSeconds(_random.Next(3, 7)) + _timing.CurTime;
        }
    }

    private void OnRefreshParts(EntityUid uid, SiliconChargerComponent component, RefreshPartsEvent args)
    {
        var chargeMod = args.PartRatings[component.ChargeSpeedPart];
        var efficiencyMod = args.PartRatings[component.ChargeEfficiencyPart];

        component.PartsChargeMulti = chargeMod * component.UpgradePartsMulti;
    }

    private void OnExamineParts(EntityUid uid, SiliconChargerComponent component, UpgradeExamineEvent args)
    {
        args.AddPercentageUpgrade("silicon-charger-chargerate-string", component.PartsChargeMulti);
    }

    #region Charger specific
    #region Step Trigger Chargers
    // When an entity starts colliding with the charger, add it to the list of entities present on the charger if it has the StepTriggerComponent.
    private void OnStartCollide(EntityUid uid, SiliconChargerComponent component, ref StartCollideEvent args)
    {
        if (!HasComp<StepTriggerComponent>(uid))
            return;

        var target = args.OtherEntity;

        if (component.PresentEntities.Contains(target))
            return;

        if (component.PresentEntities.Count >= component.MaxEntities)
        {
            _popup.PopupEntity(Loc.GetString("silicon-charger-list-full", ("charger", args.OurEntity)), target, target);
            return;
        }

        component.PresentEntities.Add(target);
    }

    // When an entity stops colliding with the charger, remove it from the list of entities present on the charger.
    private void OnEndCollide(EntityUid uid, SiliconChargerComponent component, ref EndCollideEvent args)
    {
        if (!HasComp<StepTriggerComponent>(uid))
            return;

        var target = args.OtherEntity;

        if (component.PresentEntities.Contains(target))
        {
            component.PresentEntities.Remove(target);
        }
    }
    #endregion Step Trigger Chargers
    #endregion Charger specific
}
