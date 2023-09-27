using Content.Server.Power.Components;
using Content.Server.Medical.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.White.Medical.BodyScanner;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Shared.Mobs.Components;
using Content.Server.Temperature.Components;
using Content.Server.Body.Components;
using Content.Server.Forensics;
using Content.Server.Mind.Components;
using Content.Server.Nutrition.Components;
using Content.Server.Paper;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.Nutrition.Components;
using Content.Shared.Damage.Components;
using System.Text;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameObjects;

namespace Content.Server.White.Medical.BodyScanner
{
    public sealed class BodyScannerConsoleSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly PaperSystem _paper = default!;
        [Dependency] private readonly MetaDataSystem _metaSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ActiveBodyScannerConsoleComponent, PowerChangedEvent>(OnPowerChanged);

            SubscribeLocalEvent<BodyScannerConsoleComponent, MapInitEvent>(OnMapInit);

            SubscribeLocalEvent<BodyScannerConsoleComponent, NewLinkEvent>(OnNewLink);
            SubscribeLocalEvent<BodyScannerConsoleComponent, PortDisconnectedEvent>(OnPortDisconnected);

            SubscribeLocalEvent<BodyScannerConsoleComponent, BodyScannerStartScanningMessage>(OnStartScanning);
            SubscribeLocalEvent<BodyScannerConsoleComponent, BodyScannerStartPrintingMessage>(OnStartPrinting);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var queryActiveBodyScanner = EntityQueryEnumerator<ActiveBodyScannerConsoleComponent, BodyScannerConsoleComponent>();
            while (queryActiveBodyScanner.MoveNext(out var uid, out var active, out var scan))
            {
                UpdateUserInterface(scan.Owner, scan, active);

                if (_timing.CurTime - active.StartTime < scan.ScanDuration)
                    continue;

                FinishScan(uid, scan, active);
            }

            var queryBodyInScanner = EntityQueryEnumerator<BodyInScannerBodyScannerConsoleComponent, BodyScannerConsoleComponent>();
            while (queryBodyInScanner.MoveNext(out var uid, out var bodyInScanner, out var scan))
            {
                if (bodyInScanner.MedicalCannerComponent?.BodyContainer.ContainedEntity == null)
                {
                    RemComp<BodyInScannerBodyScannerConsoleComponent>(uid);
                    UpdateUserInterface(scan.Owner, scan);
                }
            }
        }

        private void UpdateUserInterface(
            EntityUid uid,
            BodyScannerConsoleComponent? scanComponent = null,
            ActiveBodyScannerConsoleComponent? activeScanComponent = null)
        {
            if (!Resolve(uid, ref scanComponent))
                return;

            if (!_uiSystem.TryGetUi(uid, BodyScannerConsoleUIKey.Key, out var bui))
                return;

            var state = GetUserInterfaceState(scanComponent, activeScanComponent);
            scanComponent.LastScannedState = state;

            UserInterfaceSystem.SetUiState(bui, state);
        }

        private BodyScannerConsoleBoundUserInterfaceState GetUserInterfaceState(
            BodyScannerConsoleComponent consoleComponent,
            ActiveBodyScannerConsoleComponent? activeScanComponent = null)
        {
            BodyScannerConsoleBoundUserInterfaceState state = new();

            if (consoleComponent?.GeneticScanner != null && TryComp<MedicalScannerComponent>(consoleComponent.GeneticScanner, out var scanner))
            {
                state.ScannerConnected = consoleComponent.GeneticScanner != null;
                state.GeneticScannerInRange = consoleComponent.GeneticScannerInRange;

                state.CanScan = true;

                if (state.ScannerConnected && state.GeneticScannerInRange && activeScanComponent != null)
                {
                    state.CanScan = false;
                    state.CanPrint = false;

                    state.Scanning = true;
                    state.ScanTotalTime = consoleComponent.ScanDuration;
                    state.ScanTimeRemaining = consoleComponent.ScanDuration - (_timing.CurTime - activeScanComponent.StartTime);

                    return state;
                }

                var scanBody = scanner.BodyContainer.ContainedEntity;

                if (scanBody != null)
                {
                    state.CanPrint = true;

                    state.TargetEntityUid = scanBody;
                    state.EntityName = MetaData(scanBody.Value).EntityName;

                    if (TryComp<BloodstreamComponent>(scanBody, out var bloodstream))
                    {
                        state.BleedAmount = bloodstream.BleedAmount;
                        state.BloodMaxVolume = bloodstream.BloodMaxVolume;
                        state.BloodReagent = bloodstream.BloodReagent;
                        state.BloodSolution = bloodstream.BloodSolution.Clone();
                        state.BloodSolution.MaxVolume = bloodstream.BloodMaxVolume;
                        state.ChemicalMaxVolume = bloodstream.ChemicalMaxVolume;
                        state.ChemicalSolution = bloodstream.ChemicalSolution.Clone();
                        state.ChemicalSolution.MaxVolume = bloodstream.ChemicalMaxVolume;
                    }

                    if (TryComp<DnaComponent>(scanBody, out var dna))
                    {
                        state.DNA = dna.DNA;
                    }

                    if (TryComp<FingerprintComponent>(scanBody, out var fingerprint))
                    {
                        state.Fingerprint = fingerprint.Fingerprint ?? "";
                    }

                    if (TryComp<MindContainerComponent>(scanBody, out var mind))
                    {
                        state.HasMind = mind.HasMind;
                    }

                    if (TryComp<RespiratorComponent>(scanBody, out var respirator))
                    {
                        state.MaxSaturation = respirator.MaxSaturation;
                        state.MinSaturation = respirator.MinSaturation;
                        state.Saturation = respirator.Saturation;
                    }

                    if (TryComp<TemperatureComponent>(scanBody, out var temp))
                    {
                        state.HeatDamageThreshold = temp.HeatDamageThreshold;
                        state.ColdDamageThreshold = temp.ColdDamageThreshold;
                        state.CurrentTemperature = temp.CurrentTemperature;
                    }

                    if (TryComp<ThirstComponent>(scanBody, out var thirst))
                    {
                        state.CurrentThirst = thirst.CurrentThirst;
                        state.CurrentThirstThreshold = (byte) thirst.CurrentThirstThreshold;
                    }

                    if (TryComp<DamageableComponent>(scanBody, out var damageable))
                    {
                        state.TotalDamage = damageable.TotalDamage;
                        state.DamageDict = new(damageable.Damage.DamageDict);
                        state.DamagePerGroup = new(damageable.DamagePerGroup);
                    }

                    if (TryComp<HungerComponent>(scanBody, out var hunger))
                    {
                        state.CurrentHunger = hunger.CurrentHunger;
                        state.CurrentHungerThreshold = (byte) hunger.CurrentThreshold;
                    }

                    if (TryComp<MobStateComponent>(scanBody, out var mobState))
                    {
                        state.CurrentState = mobState.CurrentState;
                    }

                    if (TryComp<MobThresholdsComponent>(scanBody, out var mobThresholds))
                    {
                        foreach (var pair in mobThresholds.Thresholds)
                        {
                            if (pair.Value == Shared.Mobs.MobState.Dead)
                            {
                                state.DeadThreshold = pair.Key;
                            }
                        }
                    }

                    if (TryComp<StaminaComponent>(scanBody, out var stamina))
                    {
                        state.StaminaCritThreshold = stamina.CritThreshold;
                        state.StaminaDamage = stamina.StaminaDamage;
                    }
                }
            }

            return state;
        }

        public void FinishScan(EntityUid uid, BodyScannerConsoleComponent? component = null, ActiveBodyScannerConsoleComponent? active = null)
        {
            if (!Resolve(uid, ref component, ref active))
                return;

            _audio.PlayPvs(component.ScanFinishedSound, uid);

            RemComp<ActiveBodyScannerConsoleComponent>(uid);

            if (component?.GeneticScanner != null && TryComp<MedicalScannerComponent>(component.GeneticScanner, out var scanner))
            {
                EnsureComp<BodyInScannerBodyScannerConsoleComponent>(uid).MedicalCannerComponent = scanner;
            }

            UpdateUserInterface(uid, component);
        }

        public void OnMapInit(EntityUid uid, BodyScannerConsoleComponent component, MapInitEvent args)
        {
            if (!_uiSystem.TryGetUi(uid, BodyScannerConsoleUIKey.Key, out var bui))
                return;

            UpdateUserInterface(uid, component);
        }

        private void OnNewLink(EntityUid uid, BodyScannerConsoleComponent component, NewLinkEvent args)
        {
            if (TryComp<MedicalScannerComponent>(args.Sink, out var scanner) && args.SinkPort == BodyScannerConsoleComponent.ScannerPort)
            {
                component.GeneticScanner = args.Sink;
                scanner.ConnectedConsole = uid;
            }
        }

        private void OnPortDisconnected(EntityUid uid, BodyScannerConsoleComponent component, PortDisconnectedEvent args)
        {
            if (args.Port != BodyScannerConsoleComponent.ScannerPort)
                return;

            if (TryComp<MedicalScannerComponent>(component.GeneticScanner, out var scanner))
                scanner.ConnectedConsole = null;

            component.GeneticScanner = null;

            FinishScan(uid, component);
        }

        private void OnStartScanning(EntityUid uid, BodyScannerConsoleComponent component, BodyScannerStartScanningMessage msg)
        {
            if (component.GeneticScanner == null)
                return;

            if (!TryComp<MedicalScannerComponent>(component.GeneticScanner, out var scanner))
                return;

            if (scanner.BodyContainer.ContainedEntity == null)
                return;

            var activeComp = EnsureComp<ActiveBodyScannerConsoleComponent>(uid);
            activeComp.StartTime = _timing.CurTime;
        }

        private void OnStartPrinting(EntityUid uid, BodyScannerConsoleComponent component, BodyScannerStartPrintingMessage args)
        {
            if (!_uiSystem.TryGetUi(uid, BodyScannerConsoleUIKey.Key, out var bui))
                return;

            var state = component.LastScannedState;
            if (state == null)
                return;

            state.CanPrint = false;

            UserInterfaceSystem.SetUiState(bui, state);

            var report = Spawn(component.ReportEntityId, Transform(uid).Coordinates);
            _metaSystem.SetEntityName(report,
                Loc.GetString("body-scanner-console-report-title", ("name", state.EntityName)));

            StringBuilder text = new();
            text.AppendLine(state.EntityName);
            text.AppendLine();
            text.AppendLine(Loc.GetString("body-scanner-console-report-temperature",
                ("amount", $"{state.CurrentTemperature - 273f:F1}")));
            text.AppendLine(Loc.GetString("body-scanner-console-report-blood-level",
                ("amount", $"{state.BloodSolution.FillFraction * 100:F1}")));
            text.AppendLine(Loc.GetString("body-scanner-console-report-total-damage",
                ("amount", state.TotalDamage.ToString())));
            text.AppendLine();

            HashSet<string> shownTypes = new();

            var protos = IoCManager.Resolve<IPrototypeManager>();

            IReadOnlyDictionary<string, FixedPoint2> damagePerGroup = state.DamagePerGroup;
            IReadOnlyDictionary<string, FixedPoint2> damagePerType = state.DamageDict;

            foreach (var (damageGroupId, damageAmount) in damagePerGroup)
            {
                text.AppendLine(Loc.GetString("health-analyzer-window-damage-group-" + damageGroupId,
                    ("amount", damageAmount)));

                var group = protos.Index<DamageGroupPrototype>(damageGroupId);

                foreach (var type in group.DamageTypes)
                {
                    if (damagePerType.TryGetValue(type, out var typeAmount))
                    {
                        // If damage types are allowed to belong to more than one damage group, they may appear twice here. Mark them as duplicate.
                        if (!shownTypes.Contains(type))
                        {
                            shownTypes.Add(type);

                            text.Append(" - ");
                            text.AppendLine(Loc.GetString("health-analyzer-window-damage-type-" + type,
                                ("amount", typeAmount)));
                        }
                    }
                }

                text.AppendLine();
            }

            text.AppendLine(Loc.GetString("body-scanner-console-window-temperature-group-text"));
            text.AppendLine(Loc.GetString("body-scanner-console-window-temperature-current-temperature-text",
                ("amount", $"{state.CurrentTemperature - 273:f1}")));
            text.AppendLine(Loc.GetString(
                "body-scanner-console-window-temperature-heat-damage-threshold-temperature-text",
                ("amount", $"{state.HeatDamageThreshold - 273:f1}")));
            text.AppendLine(Loc.GetString(
                "body-scanner-console-window-temperature-cold-damage-threshold-temperature-text",
                ("amount", $"{state.ColdDamageThreshold - 273:f1}")));
            text.AppendLine();

            text.AppendLine(Loc.GetString("body-scanner-console-window-saturation-group-text"));
            text.AppendLine(Loc.GetString("body-scanner-console-window-saturation-current-saturation-text",
                ("amount", $"{state.Saturation:f1}")));
            text.AppendLine(Loc.GetString("body-scanner-console-window-saturation-maximum-saturation-text",
                ("amount", $"{state.MinSaturation:f1}")));
            text.AppendLine(Loc.GetString("body-scanner-console-window-saturation-minimum-saturation-text",
                ("amount", $"{state.MaxSaturation:f1}")));
            text.AppendLine();

            text.AppendLine(Loc.GetString("body-scanner-console-window-thirst-group-text"));
            text.AppendLine(Loc.GetString("body-scanner-console-window-thirst-current-thirst-text",
                ("amount", $"{state.CurrentThirst:f1}")));
            text.AppendLine(Loc.GetString("body-scanner-console-window-thirst-current-thirst-status-text",
                ("status",
                    Loc.GetString("body-scanner-console-window-hunger-current-hunger-status-" +
                                  state.CurrentThirstThreshold))));
            text.AppendLine();

            text.AppendLine(Loc.GetString("body-scanner-console-window-hunger-group-text"));
            text.AppendLine(Loc.GetString("body-scanner-console-window-hunger-current-hunger-text",
                ("amount", $"{state.CurrentHunger:f1}")));
            text.AppendLine(Loc.GetString("body-scanner-console-window-hunger-current-hunger-status-text",
                ("status",
                    Loc.GetString("body-scanner-console-window-thirst-current-thirst-status-" +
                                  state.CurrentHungerThreshold))));
            text.AppendLine();

            text.AppendLine(Loc.GetString("body-scanner-console-window-blood-solutions-group-text"));
            text.AppendLine(Loc.GetString("body-scanner-console-window-blood-solutions-volume-group-text",
                ("amount", $"{state.BloodSolution.Volume.Float():f1}"),
                ("maxAmount", $"{state.BloodSolution.MaxVolume.Float():f1}"),
                ("temperature", $"{state.BloodSolution.Temperature - 271:f1}")));
            state.BloodSolution.Contents.ForEach(x =>
            {
                text.Append(" - ");
                text.AppendLine($"{x.ReagentId}: {x.Quantity}");
            });
            text.AppendLine();

            text.AppendLine(Loc.GetString("body-scanner-console-window-chemical-solutions-group-text"));
            text.AppendLine(Loc.GetString("body-scanner-console-window-chemical-solutions-volume-group-text",
                ("amount", $"{state.ChemicalSolution.Volume.Float():f1}"),
                ("maxAmount", $"{state.ChemicalSolution.MaxVolume.Float():f1}"),
                ("temperature", $"{state.ChemicalSolution.Temperature - 271:f1}")));
            state.ChemicalSolution.Contents.ForEach(x =>
            {
                text.Append(" - ");
                text.AppendLine($"{x.ReagentId}: {x.Quantity}");
            });
            text.AppendLine();

            text.AppendLine(Loc.GetString("body-scanner-console-window-dna-text",
                ("value", state.DNA)));
            text.AppendLine(Loc.GetString("body-scanner-console-window-fingerprint-text",
                ("value", $"{state.Fingerprint}")));
            text.AppendLine(Loc.GetString("body-scanner-console-window-mind-text",
                ("value", $"{state.HasMind}")));

            _audio.PlayPvs(component.PrintSound, uid);
            _popup.PopupEntity(Loc.GetString("body-scanner-console-print-popup"), uid);
            _paper.SetContent(report, text.ToString());
        }

        private void OnPowerChanged(EntityUid uid, ActiveBodyScannerConsoleComponent component, ref PowerChangedEvent args)
        {
            if (args.Powered)
                return;

            RemComp<ActiveBodyScannerConsoleComponent>(uid);
        }
    }
}
