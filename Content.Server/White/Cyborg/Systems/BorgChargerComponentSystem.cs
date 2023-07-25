using Content.Server.Climbing;
using Content.Server.Power.EntitySystems;
using Content.Shared.ActionBlocker;
using Content.Shared.Body.Components;
using Content.Shared.Destructible;
using Content.Shared.DragDrop;
using Content.Shared.Movement.Events;
using Content.Shared.Verbs;
using Content.Shared.White.Cyborg.Components;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server.White.Cyborg.Systems;

/// <inheritdoc/>
public sealed class BorgChargerComponentSystem : EntitySystem
{
        [Dependency] private readonly ActionBlockerSystem _blocker = default!;
        [Dependency] private readonly ClimbSystem _climbSystem = default!;
        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly CyborgSystem _cyborg = default!;

        private const float UpdateRate = 1f;
        private float _updateDif;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BorgChargerComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<BorgChargerComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
            SubscribeLocalEvent<BorgChargerComponent, GetVerbsEvent<InteractionVerb>>(AddInsertOtherVerb);
            SubscribeLocalEvent<BorgChargerComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
            SubscribeLocalEvent<BorgChargerComponent, DestructionEventArgs>(OnDestroyed);
            SubscribeLocalEvent<BorgChargerComponent, DragDropTargetEvent>(OnDragDropOn);
            SubscribeLocalEvent<BorgChargerComponent, AnchorStateChangedEvent>(OnAnchorChanged);
            SubscribeLocalEvent<BorgChargerComponent, CanDropTargetEvent>(OnCanDragDropOn);
        }

        private void OnCanDragDropOn(EntityUid uid, BorgChargerComponent component, ref CanDropTargetEvent args)
        {
            args.Handled = true;
            args.CanDrop |= CanChargerInsert(uid, args.Dragged, component);
        }

        public bool CanChargerInsert(EntityUid uid, EntityUid target, BorgChargerComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return false;

            return HasComp<BodyComponent>(target);
        }

        private void OnComponentInit(EntityUid uid, BorgChargerComponent scannerComponent, ComponentInit args)
        {
            base.Initialize();
            scannerComponent.BodyContainer = _containerSystem.EnsureContainer<ContainerSlot>(uid, $"scanner-bodyContainer");
        }

        private void OnRelayMovement(EntityUid uid, BorgChargerComponent scannerComponent, ref ContainerRelayMovementEntityEvent args)
        {
            if (!_blocker.CanInteract(args.Entity, uid))
                return;

            EjectBody(uid, scannerComponent);
        }

        private void AddInsertOtherVerb(EntityUid uid, BorgChargerComponent component, GetVerbsEvent<InteractionVerb> args)
        {
            if (args.Using == null ||
                !args.CanAccess ||
                !args.CanInteract ||
                IsOccupied(component) ||
                !CanChargerInsert(uid, args.Using.Value, component))
                return;

            string name = "Unknown";
            if (TryComp<MetaDataComponent>(args.Using.Value, out var metadata))
                name = metadata.EntityName;

            InteractionVerb verb = new()
            {
                Act = () => InsertBody(uid, args.Target, component),
                Category = VerbCategory.Insert,
                Text = name
            };
            args.Verbs.Add(verb);
        }

        private void AddAlternativeVerbs(EntityUid uid, BorgChargerComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract)
                return;

            // Eject verb
            if (IsOccupied(component))
            {
                AlternativeVerb verb = new();
                verb.Act = () => EjectBody(uid, component);
                verb.Category = VerbCategory.Eject;
                verb.Text = Loc.GetString("medical-scanner-verb-noun-occupant");
                verb.Priority = 1; // Promote to top to make ejecting the ALT-click action
                args.Verbs.Add(verb);
            }

            // Self-insert verb
            if (!IsOccupied(component) &&
                CanChargerInsert(uid, args.User, component) &&
                _blocker.CanMove(args.User))
            {
                AlternativeVerb verb = new();
                verb.Act = () => InsertBody(uid, args.User, component);
                verb.Text = Loc.GetString("medical-scanner-verb-enter");
                args.Verbs.Add(verb);
            }
        }

        private void OnDestroyed(EntityUid uid, BorgChargerComponent scannerComponent, DestructionEventArgs args)
        {
            EjectBody(uid, scannerComponent);
        }

        private void OnDragDropOn(EntityUid uid, BorgChargerComponent scannerComponent, ref DragDropTargetEvent args)
        {
            InsertBody(uid, args.Dragged, scannerComponent);
        }

        private void OnAnchorChanged(EntityUid uid, BorgChargerComponent component, ref AnchorStateChangedEvent args)
        {

        }
        private BorgChargerVisualInfo GetStatus(EntityUid uid, BorgChargerComponent chargerComponent)
        {
            var info = new BorgChargerVisualInfo();
            info.MachineLayer = this.IsOccupied(chargerComponent) ? BorgChargerComponent.BorgChargerVisuals.Closed : BorgChargerComponent.BorgChargerVisuals.Opened;

            if (this.IsPowered(uid, EntityManager))
            {
                if (this.IsOccupied(chargerComponent))
                {
                    void LocalCharged()
                    {
                        if (chargerComponent.BodyContainer.ContainedEntity != null)
                        {
                            if (TryComp(chargerComponent.BodyContainer.ContainedEntity, out CyborgComponent? comp))
                            {
                                if (comp.Energy >= comp.MaxEnergy)
                                {
                                    info.TerminalLayer = BorgChargerComponent.BorgChargerVisuals.OccupiedCharged;
                                    return;
                                }
                            }
                        }
                        info.TerminalLayer = BorgChargerComponent.BorgChargerVisuals.Occupied;
                    }

                    LocalCharged();
                }
                else
                {
                    info.TerminalLayer = BorgChargerComponent.BorgChargerVisuals.OpenPowered;
                }
            }
            else
            {
                if (this.IsOccupied(chargerComponent))
                    info.TerminalLayer = BorgChargerComponent.BorgChargerVisuals.CloseUnpowered;
                else
                    info.TerminalLayer = BorgChargerComponent.BorgChargerVisuals.OpenUnpowered;
            }

            return info;
        }

        public bool IsOccupied(BorgChargerComponent scannerComponent)
        {
            return scannerComponent.BodyContainer.ContainedEntity != null;
        }

        private void UpdateAppearance(EntityUid uid, BorgChargerComponent chargerComponent)
        {
            if (TryComp<AppearanceComponent>(uid, out var appearance))
            {
                var info = GetStatus(uid, chargerComponent);

                _appearance.SetData(uid, BorgChargerComponent.BorgChargerVisuals.Base,
                    info.MachineLayer, appearance);
                _appearance.SetData(uid, BorgChargerComponent.BorgChargerVisuals.Light,
                    info.TerminalLayer, appearance);
            }
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            _updateDif += frameTime;
            if (_updateDif < UpdateRate)
                return;

            _updateDif -= UpdateRate;

            var query = EntityQueryEnumerator<BorgChargerComponent>();
            while(query.MoveNext(out var uid, out var charger))
            {
                var containedEnt = charger.BodyContainer.ContainedEntity;
                if (containedEnt != null)
                    _cyborg.TryChangeEnergy((EntityUid)containedEnt, charger.ChargeRate);
                UpdateAppearance(uid, charger);
            }
        }

        public void InsertBody(EntityUid uid, EntityUid toInsert, BorgChargerComponent? chargerComponent)
        {
            if (!Resolve(uid, ref chargerComponent))
                return;

            if (chargerComponent.BodyContainer.ContainedEntity != null)
                return;

            if (!HasComp<BodyComponent>(toInsert))
                return;

            chargerComponent.BodyContainer.Insert(toInsert);
            UpdateAppearance(uid, chargerComponent);

            _audio.PlayPredicted(chargerComponent.InsertSound, uid, uid);
        }

        public void EjectBody(EntityUid uid, BorgChargerComponent? chargerComponent)
        {
            if (!Resolve(uid, ref chargerComponent))
                return;

            if (chargerComponent.BodyContainer.ContainedEntity is not {Valid: true} contained)
                return;

            chargerComponent.BodyContainer.Remove(contained);
            _climbSystem.ForciblySetClimbing(contained, uid);
            UpdateAppearance(uid, chargerComponent);

            _audio.PlayPredicted(chargerComponent.RemoveSound, uid, uid);
        }
}
