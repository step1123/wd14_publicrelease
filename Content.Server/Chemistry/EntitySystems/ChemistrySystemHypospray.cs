using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Robust.Shared.GameStates;

namespace Content.Server.Chemistry.EntitySystems
{
    public sealed partial class ChemistrySystem
    {
        [Dependency] private readonly UseDelaySystem _useDelay = default!;
        [Dependency] private readonly TagSystem _tag = default!; // WD

        private void InitializeHypospray()
        {
            SubscribeLocalEvent<HyposprayComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<HyposprayComponent, MeleeHitEvent>(OnAttack);
            SubscribeLocalEvent<HyposprayComponent, SolutionChangedEvent>(OnSolutionChange);
            SubscribeLocalEvent<HyposprayComponent, UseInHandEvent>(OnUseInHand);
            SubscribeLocalEvent<HyposprayComponent, ComponentGetState>(OnHypoGetState);
            SubscribeLocalEvent<HyposprayComponent, HypoSprayDoAfterEvent>(OnHypoDoAfter);
        }

        private void OnHypoGetState(EntityUid uid, HyposprayComponent component, ref ComponentGetState args)
        {
            args.State = _solutions.TryGetSolution(uid, component.SolutionName, out var solution)
                ? new HyposprayComponentState(solution.Volume, solution.MaxVolume)
                : new HyposprayComponentState(FixedPoint2.Zero, FixedPoint2.Zero);
        }

        private void OnUseInHand(EntityUid uid, HyposprayComponent component, UseInHandEvent args)
        {
            if (args.Handled)
                return;

            TryDoInject(uid, args.User, args.User);
            args.Handled = true;
        }

        private void OnSolutionChange(EntityUid uid, HyposprayComponent component, SolutionChangedEvent args)
        {
            Dirty(component);
        }

        private void OnAfterInteract(EntityUid uid, HyposprayComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach)
                return;

            var target = args.Target;
            var user = args.User;

            TryDoInject(uid, target, user);
        }

        private void OnAttack(EntityUid uid, HyposprayComponent component, MeleeHitEvent args)
        {
            if (!args.HitEntities.Any())
                return;

            if (TryDoInject(uid, args.HitEntities.First(), args.User, hard: true))
            {
                EntityManager.RemoveComponent<MeleeWeaponComponent>(args.Weapon);
            }
        }

        public bool TryDoInject(EntityUid uid, EntityUid? target, EntityUid user, HyposprayComponent? component = null,
            bool hard = false)
        {
            if (!Resolve(uid, ref component))
                return false;

            if (!EligibleEntity(target, _entMan))
                return false;

            if (TryComp(uid, out UseDelayComponent? delayComp) && _useDelay.ActiveDelay(uid, delayComp))
                return false;

            string? msgFormat = null;

            if (target == user)
                msgFormat = "hypospray-component-inject-self-message";
            else if (EligibleEntity(user, _entMan) && _interaction.TryRollClumsy(user, component.ClumsyFailChance))
            {
                msgFormat = "hypospray-component-inject-self-clumsy-message";
                target = user;
            }

            _solutions.TryGetSolution(uid, component.SolutionName, out var hypoSpraySolution);

            if (hypoSpraySolution == null || hypoSpraySolution.Volume == 0)
            {
                _popup.PopupCursor(Loc.GetString("hypospray-component-empty-message"), user);
                return true;
            }

            if (!_solutions.TryGetInjectableSolution(target.Value, out var targetSolution))
            {
                _popup.PopupCursor(
                    Loc.GetString("hypospray-cant-inject", ("target", Identity.Entity(target.Value, _entMan))), user);
                return false;
            }

            // WD EDIT Start

            if (hard == false && _inventorySystem.TryGetSlotEntity(target.Value, "outerClothing", out var suit) &&
                _tag.HasTag(suit.Value, "Hardsuit") &&
                _inventorySystem.TryGetSlotEntity(target.Value, "head", out var helmet) &&
                _tag.HasAnyTag(helmet.Value, new List<string> {"HardsuitHelmet", "HelmetEVA"}))

        {
                // If the target is wearing a pressure protection component, let's add a delay.
                msgFormat = Loc.GetString("hypospray-component-inject-self-message-space");

                var delay = _random.NextFloat() / 3f + 0.4f;

                if (delayComp is not null)
                    _useDelay.BeginDelay(uid, delayComp);

                // Get transfer amount. May be smaller than component.TransferAmount if not enough room
                var realTransferAmountDoAfter = FixedPoint2.Min(component.TransferAmount, targetSolution.AvailableVolume);

                if (realTransferAmountDoAfter <= 0)
                {
                    _popup.PopupCursor(Loc.GetString("hypospray-component-transfer-already-full-message", ("owner", target)), user);
                    return true;
                }

                _doAfter.TryStartDoAfter(new DoAfterArgs(user, delay, new HypoSprayDoAfterEvent()
                {
                    HypoSpraySolution = hypoSpraySolution,
                    TargetSolution = targetSolution,
                    RealTransferAmount = realTransferAmountDoAfter
                }, uid, target: target.Value, used: uid)
                {
                    DistanceThreshold = SharedInteractionSystem.InteractionRange
                });

                // For the user
                _popup.PopupCursor(Loc.GetString(msgFormat), user);

                // For the target
                if (user != target)
                {
                    var userName = Identity.Entity(user, EntityManager);
                    _popup.PopupEntity(Loc.GetString("injector-component-injecting-target-suit",("user", userName)), user, target.Value);
                }

                return true;
        }

            // WD EDIT End

            _popup.PopupCursor(Loc.GetString(msgFormat ?? "hypospray-component-inject-other-message", ("other", target)), user);

            if (target != user)
            {
                _popup.PopupEntity(Loc.GetString("hypospray-component-feel-prick-message"), target.Value, target.Value);
                // TODO: This should just be using melee attacks...
                // meleeSys.SendLunge(angle, user);
            }

            _audio.PlayPvs(component.InjectSound, user);

            // Medipens and such use this system and don't have a delay, requiring extra checks
            // BeginDelay function returns if item is already on delay
            if (delayComp is not null)
                _useDelay.BeginDelay(uid, delayComp);

            // Get transfer amount. May be smaller than component.TransferAmount if not enough room
            var realTransferAmount = FixedPoint2.Min(component.TransferAmount, targetSolution.AvailableVolume);

            if (realTransferAmount <= 0)
            {
                _popup.PopupCursor(Loc.GetString("hypospray-component-transfer-already-full-message", ("owner", target)), user);
                return true;
            }

            // WD EDIT Start

            if (DoInject(uid, target, user, hypoSpraySolution, targetSolution, realTransferAmount))
            {
                return true;
            }

            return true;
        }

        private bool DoInject(EntityUid uid, EntityUid? target, EntityUid user, Solution hypoSpraySolution, Solution targetSolution, FixedPoint2 realTransferAmount)
        {
            // Move units from attackSolution to targetSolution
            var removedSolution = _solutions.SplitSolution(uid, hypoSpraySolution, realTransferAmount);

            if (!targetSolution.CanAddSolution(removedSolution))
                return true;

            if (target == null)
                return false;

            _reactiveSystem.DoEntityReaction(target.Value, removedSolution, ReactionMethod.Injection);
            _solutions.TryAddSolution(target.Value, targetSolution, removedSolution);

            // same LogType as syringes...
            _adminLogger.Add(LogType.ForceFeed, $"{_entMan.ToPrettyString(user):user} injected {_entMan.ToPrettyString(target.Value):target} with a solution {SolutionContainerSystem.ToPrettyString(removedSolution):removedSolution} using a {_entMan.ToPrettyString(uid):using}");

            return true;
        }

        private void OnHypoDoAfter(EntityUid uid, HyposprayComponent component, HypoSprayDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled || args.Args.Target == null)
                return;

            if (DoInject(uid, args.Args.Target, args.Args.User, args.HypoSpraySolution, args.TargetSolution, args.RealTransferAmount))
            {
                args.Handled = true;
                _audio.PlayPvs(component.InjectSound, args.Args.Target.Value);
            }
        }

        // WD EDIT End


        static bool EligibleEntity([NotNullWhen(true)] EntityUid? entity, IEntityManager entMan)
        {
            // TODO: Does checking for BodyComponent make sense as a "can be hypospray'd" tag?
            // In SS13 the hypospray ONLY works on mobs, NOT beakers or anything else.

            return entMan.HasComponent<SolutionContainerManagerComponent>(entity)
                && entMan.HasComponent<MobStateComponent>(entity);
        }
    }
}
