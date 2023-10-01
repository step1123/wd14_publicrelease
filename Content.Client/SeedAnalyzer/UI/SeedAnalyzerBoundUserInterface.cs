using Content.Shared.Botany;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.SeedAnalyzer.UI
{
    [UsedImplicitly]
    public sealed class SeedAnalyzerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private SeedAnalyzerWindow? _window;

        public SeedAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();
            _window = new SeedAnalyzerWindow
            {
                Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName,
            };
            _window.OnClose += Close;
            _window.OpenCentered();
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is not SeedAnalyzerScannedUserMessage cast)
                return;

            _window.Populate(cast);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_window != null)
                _window.OnClose -= Close;

            _window?.Dispose();
        }
    }
}
