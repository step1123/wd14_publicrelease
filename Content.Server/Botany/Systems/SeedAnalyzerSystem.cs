using Content.Server.Botany.Components;
using Content.Server.PowerCell;
using Content.Shared.FixedPoint;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Botany;
using Content.Shared.Mobs.Components; //probably useless
using Robust.Server.GameObjects;

namespace Content.Server.Botany
{
    public sealed class SeedAnalyzerSystem : EntitySystem
    {
        [Dependency] private readonly PowerCellSystem _cell = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SeedAnalyzerComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<SeedAnalyzerComponent, SeedAnalyzerDoAfterEvent>(OnDoAfter);
        }

        private void OnAfterInteract(EntityUid uid, SeedAnalyzerComponent seedAnalyzer, AfterInteractEvent args)
        {
            if (args.Target == null || !args.CanReach || !HasComp<PlantHolderComponent>(args.Target) || !_cell.HasActivatableCharge(uid, user: args.User))
                return;

            _audio.PlayPvs(seedAnalyzer.ScanningBeginSound, uid); // maybe healthAnalyzer.ScanningBeginSound

            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(args.User, seedAnalyzer.ScanDelay, new SeedAnalyzerDoAfterEvent(), uid, target: args.Target, used: uid) //maybe healthAnalyzer.ScanDelay
            {
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true
            });
        }

        private void OnDoAfter(EntityUid uid, SeedAnalyzerComponent component, DoAfterEvent args)
        {
            if (args.Handled || args.Cancelled || args.Args.Target == null || !_cell.TryUseActivatableCharge(uid, user: args.User))
                return;

            _audio.PlayPvs(component.ScanningEndSound, args.Args.User);

            UpdateScannedSeed(uid, args.Args.User, args.Args.Target.Value, component);
            args.Handled = true;
        }

        private void OpenUserInterface(EntityUid user, SeedAnalyzerComponent seedAnalyzer)
        {
            if (!TryComp<ActorComponent>(user, out var actor) || seedAnalyzer.UserInterface == null)
                return;

            _uiSystem.OpenUi(seedAnalyzer.UserInterface, actor.PlayerSession);
        }

        public void UpdateScannedSeed(EntityUid uid, EntityUid user, EntityUid? target, SeedAnalyzerComponent? seedAnalyzer)
        {
            if (!Resolve(uid, ref seedAnalyzer))
                return;

            if (target == null || seedAnalyzer.UserInterface == null)
                return;

            if (!TryComp<PlantHolderComponent>(target, out var plant))
                return;

            if (plant!.Seed == null)
            {
                return;
            }

            int zero = 0;
            string nonameseed = "None";
            Dictionary<string, FixedPoint2> emptydic = new Dictionary<string, FixedPoint2>();
            var zero2 = FixedPoint2.New(zero);
            emptydic?.Add("No chemicals", zero2!);
            Dictionary<string, FixedPoint2> passchems = new Dictionary<string, FixedPoint2>();

            if (plant?.Seed?.Chemicals != null)
            {
                foreach (var (chem, quantity) in plant.Seed.Chemicals)
                {
                    var amount = FixedPoint2.New(quantity.Min);
                    amount += FixedPoint2.New(plant.Seed.Potency / quantity.PotencyDivisor);
                    amount = FixedPoint2.New((int) MathHelper.Clamp(amount.Float(), quantity.Min, quantity.Max));
                    passchems?.Add(chem, amount);
                }
            }

            OpenUserInterface(user, seedAnalyzer);

            _uiSystem.SendUiMessage(seedAnalyzer.UserInterface, new SeedAnalyzerScannedUserMessage(target,
            plant!.Seed?.Yield,
            plant != null ? plant.Seed?.Production : float.NaN,
            plant != null ? plant.Seed?.Lifespan : float.NaN,
            plant != null ? plant.Seed?.Maturation : float.NaN,
            plant != null ? plant.Seed?.Endurance : float.NaN,
            plant != null ? plant.Seed?.Potency : float.NaN,
            plant != null ? plant.Seed?.Viable : false,
            plant != null ? plant.Seed?.TurnIntoKudzu : false,
            plant != null ? plant.Seed?.Seedless : false,
            plant != null ? plant.Seed?.DisplayName : nonameseed,
            passchems != null ? passchems : emptydic,
            plant != null ? plant.Seed?.Ligneous : false,
            plant != null ? plant.Seed?.CanScream : false,
            plant != null ? plant.Seed?.Slip : false,
            plant != null ? plant.Seed?.Bioluminescent : false,
            plant != null ? plant.Seed?.Sentient : false));
        }
    }
}
