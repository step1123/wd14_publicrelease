using Content.Server.Administration.Logs;
using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.DoAfter; // WD
using Content.Server.Nutrition.EntitySystems; // WD
using Content.Server.Popups;
using Content.Shared.ActionBlocker; // WD
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid; // WD
using Content.Shared.IdentityManagement; // WD
using Content.Shared.Interaction; // WD
using Content.Shared.Inventory; // WD
using Content.Shared.Mobs; // WD
using Content.Shared.Mobs.Components; // WD
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups; // WD
using Content.Shared.White.CPR.Events; // WD
using JetBrains.Annotations;
using Robust.Server.GameObjects; // WD
using Robust.Shared.Audio; // WD
// WD removed
using Robust.Shared.Timing;

namespace Content.Server.Body.Systems
{
    [UsedImplicitly]
    public sealed class RespiratorSystem : EntitySystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly AlertsSystem _alertsSystem = default!;
        [Dependency] private readonly AtmosphereSystem _atmosSys = default!;
        [Dependency] private readonly BodySystem _bodySystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSys = default!;
        [Dependency] private readonly LungSystem _lungSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!; // WD
        [Dependency] private readonly ActionBlockerSystem _blocker = default!; // WD
        [Dependency] private readonly AudioSystem _audio = default!; // WD
        [Dependency] private readonly DoAfterSystem _doAfter = default!; // WD
        [Dependency] private readonly DamageableSystem _damageable = default!; // WD

        public override void Initialize()
        {
            base.Initialize();

            // We want to process lung reagents before we inhale new reagents.
            UpdatesAfter.Add(typeof(MetabolizerSystem));
            SubscribeLocalEvent<RespiratorComponent, ApplyMetabolicMultiplierEvent>(OnApplyMetabolicMultiplier);
            SubscribeLocalEvent<RespiratorComponent, InteractHandEvent>(OnHandInteract); // WD
            SubscribeLocalEvent<RespiratorComponent, CPREndedEvent>(OnCPRDoAfterEnd); // WD
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var (respirator, body) in EntityManager.EntityQuery<RespiratorComponent, BodyComponent>())
            {
                var uid = respirator.Owner;

                if (_mobState.IsDead(uid))
                {
                    continue;
                }

                respirator.AccumulatedFrametime += frameTime;

                if (respirator.AccumulatedFrametime < respirator.CycleDelay)
                    continue;
                respirator.AccumulatedFrametime -= respirator.CycleDelay;
                UpdateSaturation(respirator.Owner, -respirator.CycleDelay, respirator);

                if (!_mobState.IsIncapacitated(uid)) // cannot breathe in crit.
                {
                    switch (respirator.Status)
                    {
                        case RespiratorStatus.Inhaling:
                            Inhale(uid, body);
                            respirator.Status = RespiratorStatus.Exhaling;
                            break;
                        case RespiratorStatus.Exhaling:
                            Exhale(uid, body);
                            respirator.Status = RespiratorStatus.Inhaling;
                            break;
                    }
                }

                if (respirator.Saturation < respirator.SuffocationThreshold)
                {
                    if (_gameTiming.CurTime >= respirator.LastGaspPopupTime + respirator.GaspPopupCooldown)
                    {
                        respirator.LastGaspPopupTime = _gameTiming.CurTime;
                        _popupSystem.PopupEntity(Loc.GetString("lung-behavior-gasp"), uid);
                    }

                    TakeSuffocationDamage(uid, respirator);
                    respirator.SuffocationCycles += 1;
                    continue;
                }

                StopSuffocation(uid, respirator);
                respirator.SuffocationCycles = 0;
            }
        }

        public void Inhale(EntityUid uid, BodyComponent? body = null)
        {
            if (!Resolve(uid, ref body, false))
                return;

            var organs = _bodySystem.GetBodyOrganComponents<LungComponent>(uid, body);

            // Inhale gas
            var ev = new InhaleLocationEvent();
            RaiseLocalEvent(uid, ev, false);

            ev.Gas ??= _atmosSys.GetContainingMixture(uid, false, true);

            if (ev.Gas == null)
            {
                return;
            }

            var actualGas = ev.Gas.RemoveVolume(Atmospherics.BreathVolume);

            var lungRatio = 1.0f / organs.Count;
            var gas = organs.Count == 1 ? actualGas : actualGas.RemoveRatio(lungRatio);
            foreach (var (lung, _) in organs)
            {
                // Merge doesn't remove gas from the giver.
                _atmosSys.Merge(lung.Air, gas);
                _lungSystem.GasToReagent(lung.Owner, lung);
            }
        }

        public void Exhale(EntityUid uid, BodyComponent? body = null)
        {
            if (!Resolve(uid, ref body, false))
                return;

            var organs = _bodySystem.GetBodyOrganComponents<LungComponent>(uid, body);

            // exhale gas

            var ev = new ExhaleLocationEvent();
            RaiseLocalEvent(uid, ev, false);

            if (ev.Gas == null)
            {
                ev.Gas = _atmosSys.GetContainingMixture(uid, false, true);

                // Walls and grids without atmos comp return null. I guess it makes sense to not be able to exhale in walls,
                // but this also means you cannot exhale on some grids.
                ev.Gas ??= GasMixture.SpaceGas;
            }

            var outGas = new GasMixture(ev.Gas.Volume);
            foreach (var (lung, _) in organs)
            {
                _atmosSys.Merge(outGas, lung.Air);
                lung.Air.Clear();
                lung.LungSolution.RemoveAllSolution();
            }

            _atmosSys.Merge(ev.Gas, outGas);
        }

        private void TakeSuffocationDamage(EntityUid uid, RespiratorComponent respirator)
        {
            if (respirator.SuffocationCycles == 2)
                _adminLogger.Add(LogType.Asphyxiation, $"{ToPrettyString(uid):entity} started suffocating");

            if (respirator.SuffocationCycles >= respirator.SuffocationCycleThreshold)
            {
                _alertsSystem.ShowAlert(uid, AlertType.LowOxygen);
            }

            _damageableSys.TryChangeDamage(uid, respirator.Damage, true, false);
        }

        private void StopSuffocation(EntityUid uid, RespiratorComponent respirator)
        {
            if (respirator.SuffocationCycles >= 2)
                _adminLogger.Add(LogType.Asphyxiation, $"{ToPrettyString(uid):entity} stopped suffocating");

            _alertsSystem.ClearAlert(uid, AlertType.LowOxygen);

            _damageableSys.TryChangeDamage(uid, respirator.DamageRecovery, true);
        }

        public void UpdateSaturation(EntityUid uid, float amount,
            RespiratorComponent? respirator = null)
        {
            if (!Resolve(uid, ref respirator, false))
                return;

            respirator.Saturation += amount;
            respirator.Saturation =
                Math.Clamp(respirator.Saturation, respirator.MinSaturation, respirator.MaxSaturation);
        }

        private void OnApplyMetabolicMultiplier(EntityUid uid, RespiratorComponent component,
            ApplyMetabolicMultiplierEvent args)
        {
            if (args.Apply)
            {
                component.CycleDelay *= args.Multiplier;
                component.Saturation *= args.Multiplier;
                component.MaxSaturation *= args.Multiplier;
                component.MinSaturation *= args.Multiplier;
                return;
            }

            // This way we don't have to worry about it breaking if the stasis bed component is destroyed
            component.CycleDelay /= args.Multiplier;
            component.Saturation /= args.Multiplier;
            component.MaxSaturation /= args.Multiplier;
            component.MinSaturation /= args.Multiplier;
            // Reset the accumulator properly
            if (component.AccumulatedFrametime >= component.CycleDelay)
                component.AccumulatedFrametime = component.CycleDelay;
        }

        // WD start
        private void OnHandInteract(EntityUid uid, RespiratorComponent component, InteractHandEvent args)
        {
            if (CanCPR(uid, component, args.User))
                DoCPR(uid, component, args.User);

            args.Handled = true;
        }

        private bool CanCPR(EntityUid target, RespiratorComponent comp, EntityUid user)
        {
            if (!_blocker.CanInteract(user, target))
                return false;

            if (target == user)
                return false;

            if (comp.CPRPerformedBy != null && comp.CPRPerformedBy != user)
                return false;

            if (!TryComp<HumanoidAppearanceComponent>(target, out _))
                return false;

            if (!TryComp(target, out MobStateComponent? targetState))
                return false;

            if (targetState.CurrentState == MobState.Dead)
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-too-late", ("target", Identity.Entity(target, EntityManager))), target, user);
                return false;
            }

            if (targetState.CurrentState != MobState.Critical)
                return false;

            if (_inventorySystem.TryGetSlotEntity(user, "mask", out var maskUidUser) &&
                EntityManager.TryGetComponent<IngestionBlockerComponent>(maskUidUser, out var blockerUser) &&
                blockerUser.Enabled)
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-mask-block-user"), user, user);
                return false;
            }

            if (_inventorySystem.TryGetSlotEntity(target, "mask", out var maskUidTarget) &&
                EntityManager.TryGetComponent<IngestionBlockerComponent>(maskUidTarget, out var blockerTarget) &&
                blockerTarget.Enabled)
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-mask-block-target", ("target", Identity.Entity(target, EntityManager))), target, user);
                return false;
            }

            return true;
        }

        private void DoCPR(EntityUid target, RespiratorComponent comp, EntityUid user)
        {

            var doAfterEventArgs = new DoAfterArgs(user, comp.CycleDelay * 4, new CPREndedEvent(user, target), target, target: target)
            {
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                BreakOnDamage = true,
                NeedHand = true,
                BreakOnHandChange = true
            };

            if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-failed"), user, user);
                return;
            }

            comp.CPRPerformedBy = user;

            _popupSystem.PopupEntity(Loc.GetString("cpr-started", ("target", Identity.Entity(target, EntityManager)), ("user", Identity.Entity(user, EntityManager))), target, PopupType.Medium);
            comp.CPRPlayingStream = _audio.PlayPvs(comp.CPRSound, target, audioParams: AudioParams.Default.WithVolume(-3f).WithLoop(true));

            _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(user):entity} начал произовдить СЛР на {ToPrettyString(target):entity}");
        }

        private void OnCPRDoAfterEnd(EntityUid uid, RespiratorComponent component, CPREndedEvent args)
        {
            if (args.Handled)
                return;

            if (args.Cancelled || !TryComp<MobStateComponent>(args.Target, out var targetState) || targetState!.CurrentState != MobState.Critical)
            {
                component.CPRPlayingStream?.Stop();
                component.CPRPerformedBy = null;
                _popupSystem.PopupEntity(Loc.GetString("cpr-failed"), args.User, args.User);
                _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.User):entity} не удалось произвести СЛР на {ToPrettyString(args.Target):entity}");
                return;
            }

            args.Handled = true;

            _damageable.TryChangeDamage(uid, -component.Damage * 2, true, false);

            _popupSystem.PopupEntity(Loc.GetString("cpr-cycle-ended", ("target", Identity.Entity(uid, EntityManager)), ("user", Identity.Entity(args.User, EntityManager))), uid);

            _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.User):entity} произвёл СЛР на {ToPrettyString(args.Target):entity}");

            if (CanCPR(args.Target, component, args.User))
                args.Repeat = true;
            else
                component.CPRPerformedBy = null;
        }
        //WD end
    }
}

public sealed class InhaleLocationEvent : EntityEventArgs
{
    public GasMixture? Gas;
}

public sealed class ExhaleLocationEvent : EntityEventArgs
{
    public GasMixture? Gas;
}
