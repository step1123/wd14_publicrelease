using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Chemistry.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Dispenser;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for reagent dispensers.
    /// <seealso cref="ReagentDispenserComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class ReagentDispenserSystem : EntitySystem
    {
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!; // WD
        [Dependency] private readonly ChemMasterSystem _chemMasterSystem = default!; // WD

        public override void Initialize()
        {
            base.Initialize();

            // WD START
            SubscribeLocalEvent<ReagentDispenserComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<ReagentDispenserComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<ReagentDispenserComponent, NewLinkEvent>(OnNewLink);
            SubscribeLocalEvent<ReagentDispenserComponent, PortDisconnectedEvent>(OnPortDisconnected);
            SubscribeLocalEvent<ReagentDispenserComponent, AnchorStateChangedEvent>(OnAnchorChanged);
            // WD END

            SubscribeLocalEvent<ReagentDispenserComponent, ComponentStartup>((_, comp, _) => UpdateUiState(comp));
            SubscribeLocalEvent<ReagentDispenserComponent, SolutionChangedEvent>((_, comp, _) => UpdateUiState(comp));
            SubscribeLocalEvent<ReagentDispenserComponent, EntInsertedIntoContainerMessage>((_, comp, _) => UpdateUiState(comp));
            SubscribeLocalEvent<ReagentDispenserComponent, EntRemovedFromContainerMessage>((_, comp, _) => UpdateUiState(comp));
            SubscribeLocalEvent<ReagentDispenserComponent, BoundUIOpenedEvent>((_, comp, _) => UpdateUiState(comp));
            SubscribeLocalEvent<ReagentDispenserComponent, GotEmaggedEvent>(OnEmagged);

            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserSetDispenseAmountMessage>(OnSetDispenseAmountMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserDispenseReagentMessage>(OnDispenseReagentMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserClearContainerSolutionMessage>(OnClearContainerSolutionMessage);
        }

        // WD START
        private void OnInit(EntityUid uid, ReagentDispenserComponent component, ComponentInit args)
        {
            _signalSystem.EnsureSourcePorts(uid, ReagentDispenserComponent.ChemMasterPort);
        }

        private void OnMapInit(EntityUid uid, ReagentDispenserComponent component, MapInitEvent args)
        {
            if (!TryComp<DeviceLinkSourceComponent>(uid, out var receiver))
                return;

            foreach (var port in receiver.Outputs.Values.SelectMany(ports => ports))
            {
                if (!TryComp<ChemMasterComponent>(port, out var master))
                    continue;

                UpdateConnection(uid, port, component, master);
                break;
            }
        }

        private void OnNewLink(EntityUid uid, ReagentDispenserComponent component, NewLinkEvent args)
        {
            if (TryComp<ChemMasterComponent>(args.Sink, out var master) && args.SourcePort == ReagentDispenserComponent.ChemMasterPort)
                UpdateConnection(uid, args.Sink, component, master);
        }

        private void OnPortDisconnected(EntityUid uid, ReagentDispenserComponent component, PortDisconnectedEvent args)
        {
            if (args.Port != ReagentDispenserComponent.ChemMasterPort)
                return;

            component.ChemMaster = null;
            component.ChemMasterInRange = false;
        }

        private void OnAnchorChanged(EntityUid uid, ReagentDispenserComponent component, ref AnchorStateChangedEvent args)
        {
            if (args.Anchored)
                RecheckConnections(uid, component);
        }

        public void UpdateConnection(EntityUid dispenser, EntityUid chemMaster,
            ReagentDispenserComponent? dispenserComp = null, ChemMasterComponent? chemMasterComp = null)
        {
            if (!Resolve(dispenser, ref dispenserComp) || !Resolve(chemMaster, ref chemMasterComp))
                return;

            if (dispenserComp.ChemMaster.HasValue && dispenserComp.ChemMaster.Value != chemMaster &&
                TryComp(dispenserComp.ChemMaster, out ChemMasterComponent? oldMaster))
            {
                oldMaster.ConnectedDispenser = null;
            }

            if (chemMasterComp.ConnectedDispenser.HasValue && chemMasterComp.ConnectedDispenser.Value != dispenser &&
                TryComp(dispenserComp.ChemMaster, out ReagentDispenserComponent? oldDispenser))
            {
                oldDispenser.ChemMaster = null;
                oldDispenser.ChemMasterInRange = false;
            }

            dispenserComp.ChemMaster = chemMaster;
            chemMasterComp.ConnectedDispenser = dispenser;

            RecheckConnections(dispenser, dispenserComp);
        }

        private void RecheckConnections(EntityUid dispenser, ReagentDispenserComponent? component = null)
        {
            if (!Resolve(dispenser, ref component))
                return;

            if (component.ChemMaster == null)
            {
                component.ChemMasterInRange = false;
                return;
            }

            Transform(component.ChemMaster.Value).Coordinates
                .TryDistance(EntityManager, Transform(dispenser).Coordinates, out var distance);
            component.ChemMasterInRange = distance <= 1.5f;
        }
        // WD END

        private void UpdateUiState(ReagentDispenserComponent reagentDispenser)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser.Owner, SharedReagentDispenser.OutputSlotName);
            var outputContainerInfo = BuildOutputContainerInfo(outputContainer);

            var inventory = GetInventory(reagentDispenser);

            var state = new ReagentDispenserBoundUserInterfaceState(outputContainerInfo, inventory, reagentDispenser.DispenseAmount);
            _userInterfaceSystem.TrySetUiState(reagentDispenser.Owner, ReagentDispenserUiKey.Key, state);
        }

        private ContainerInfo? BuildOutputContainerInfo(EntityUid? container)
        {
            if (container is not { Valid: true })
                return null;

            if (_solutionContainerSystem.TryGetFitsInDispenser(container.Value, out var solution))
            {
                var reagents = solution.Contents.Select(reagent => (reagent.ReagentId, reagent.Quantity)).ToList();
                return new ContainerInfo(Name(container.Value), true, solution.Volume, solution.MaxVolume, reagents);
            }

            return null;
        }

        private List<string> GetInventory(ReagentDispenserComponent reagentDispenser)
        {
            var inventory = new List<string>();

            if (reagentDispenser.PackPrototypeId is not null
                && _prototypeManager.TryIndex(reagentDispenser.PackPrototypeId, out ReagentDispenserInventoryPrototype? packPrototype))
            {
                inventory.AddRange(packPrototype.Inventory);
            }

            if (HasComp<EmaggedComponent>(reagentDispenser.Owner)
                && reagentDispenser.EmagPackPrototypeId is not null
                && _prototypeManager.TryIndex(reagentDispenser.EmagPackPrototypeId, out ReagentDispenserInventoryPrototype? emagPackPrototype))
            {
                inventory.AddRange(emagPackPrototype.Inventory);
            }

            return inventory;
        }

        private void OnEmagged(EntityUid uid, ReagentDispenserComponent reagentDispenser, ref GotEmaggedEvent args)
        {
            // adding component manually to have correct state
            EntityManager.AddComponent<EmaggedComponent>(uid);
            UpdateUiState(reagentDispenser);
            args.Handled = true;
        }

        private void OnSetDispenseAmountMessage(EntityUid uid, ReagentDispenserComponent reagentDispenser, ReagentDispenserSetDispenseAmountMessage message)
        {
            reagentDispenser.DispenseAmount = message.ReagentDispenserDispenseAmount;
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void OnDispenseReagentMessage(EntityUid uid, ReagentDispenserComponent reagentDispenser, ReagentDispenserDispenseReagentMessage message)
        {
            // Ensure that the reagent is something this reagent dispenser can dispense.
            if (!GetInventory(reagentDispenser).Contains(message.ReagentId))
                return;

            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser.Owner, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not {Valid: true} || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution))
            { // WD EDIT START
                if (!reagentDispenser.ChemMasterInRange ||
                    !TryComp(reagentDispenser.ChemMaster, out ChemMasterComponent? chemMaster) ||
                    !_solutionContainerSystem.TryGetSolution(reagentDispenser.ChemMaster,
                        SharedChemMaster.BufferSolutionName, out var bufferSolution))
                    return;

                bufferSolution.AddReagent(message.ReagentId, FixedPoint2.New((int)reagentDispenser.DispenseAmount));
                _chemMasterSystem.UpdateUiState(chemMaster);
                ClickSound(reagentDispenser);

                return;
            } // WD EDIT END

            if (_solutionContainerSystem.TryAddReagent(outputContainer.Value, solution, message.ReagentId, (int)reagentDispenser.DispenseAmount, out var dispensedAmount)
                && message.Session.AttachedEntity is not null)
            {
                _adminLogger.Add(LogType.ChemicalReaction, LogImpact.Medium,
                    $"{ToPrettyString(message.Session.AttachedEntity.Value):player} dispensed {dispensedAmount}u of {message.ReagentId} into {ToPrettyString(outputContainer.Value):entity}");
            }

            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void OnClearContainerSolutionMessage(EntityUid uid, ReagentDispenserComponent reagentDispenser, ReagentDispenserClearContainerSolutionMessage message)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser.Owner, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not {Valid: true} || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution))
                return;

            _solutionContainerSystem.RemoveAllSolution(outputContainer.Value, solution);
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void ClickSound(ReagentDispenserComponent reagentDispenser)
        {
            _audioSystem.PlayPvs(reagentDispenser.ClickSound, reagentDispenser.Owner, AudioParams.Default.WithVolume(-2f));
        }
    }
}
