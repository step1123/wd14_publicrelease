using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Server.White.Medical.BodyScanner
{
    [RegisterComponent]
    public sealed partial class ActiveBodyScannerConsoleComponent : Component
    {
        /// <summary>
        /// When did the scanning start?
        /// </summary>
        [DataField("startTime", customTypeSerializer: typeof(TimespanSerializer))]
        public TimeSpan StartTime;

        /// <summary>
        /// What is being scanned?
        /// </summary>
        [ViewVariables]
        public EntityUid PatientUid;
    }
}
