using Content.Shared.White.Medical.BodyScanner;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.White.Medical.BodyScanner
{
    [RegisterComponent]
    public sealed partial class BodyScannerConsoleComponent : Component
    {
        public const string ScannerPort = "MedicalScannerReceiver";

        /// <summary>
        /// Genetic scanner. Can be null if not linked.
        /// </summary>
        [ViewVariables]
        public EntityUid? GeneticScanner;

        /// <summary>
        /// Maximum distance between body scanner console and one if its machines.
        /// </summary>
        [DataField("maxDistance")]
        public float MaxDistance = 4f;

        public bool GeneticScannerInRange = true;

        /// <summary>
        /// How long it takes to scan.
        /// </summary>
        [ViewVariables, DataField("scanDuration", customTypeSerializer: typeof(TimespanSerializer))]
        public TimeSpan ScanDuration = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Sound to play on scan finished.
        /// </summary>
        [DataField("scanFinishedSound")]
        public SoundSpecifier ScanFinishedSound = new SoundPathSpecifier("/Audio/Machines/scan_finish.ogg");

        /// <summary>
        /// Sound to play when printing.
        /// </summary>
        [DataField("printSound")]
        public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/diagnoser_printing.ogg");

        [ViewVariables]
        public BodyScannerConsoleBoundUserInterfaceState? LastScannedState;

        /// <summary>
        /// The entity spawned by a report.
        /// </summary>
        [DataField("reportEntityId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ReportEntityId = "Paper";
    }
}
