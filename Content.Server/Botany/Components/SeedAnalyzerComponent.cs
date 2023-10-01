using Content.Server.UserInterface;
using Content.Shared.Botany;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;

namespace Content.Server.Botany.Components
{
    /// <summary>
    ///    After scanning, retrieves the target Uid to use with its related UI.
    /// </summary>
    [RegisterComponent]
    public sealed class SeedAnalyzerComponent : Component
    {
        /// <summary>
        /// How long it takes to scan a seed.
        /// </summary>
        [DataField("scanDelay")]
        public float ScanDelay = 0.8f;

        public BoundUserInterface? UserInterface => Owner.GetUIOrNull(SeedAnalyzerUiKey.Key);

        /// <summary>
        ///     Sound played on scanning begin
        /// </summary>
        [DataField("scanningBeginSound")]
        public SoundSpecifier? ScanningBeginSound;

        /// <summary>
        ///     Sound played on scanning end
        /// </summary>
        [DataField("scanningEndSound")]
        public SoundSpecifier? ScanningEndSound;
    }
}
