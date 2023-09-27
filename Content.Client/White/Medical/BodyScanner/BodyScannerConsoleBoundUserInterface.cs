using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Content.Shared.White.Medical.BodyScanner;

namespace Content.Client.White.Medical.BodyScanner
{
    [UsedImplicitly]
    public sealed class BodyScannerConsoleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private BodyScannerConsoleWindow? _window;

        public BodyScannerConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();
            _window = new BodyScannerConsoleWindow();

            _window.OnClose += Close;
            _window.OpenCentered();

            _window.OnScanButtonPressed += () => StartScanning();
            _window.OnPrintButtonPressed += () => StartPrinting();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            switch(state)
            {
                case BodyScannerConsoleBoundUserInterfaceState msg:
                    _window?.UpdateUserInterface(msg);
                    break;
            }
        }

        public void StartScanning()
        {
            SendMessage(new BodyScannerStartScanningMessage());
        }

        public void StartPrinting()
        {
            SendMessage(new BodyScannerStartPrintingMessage());
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
