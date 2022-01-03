#nullable enable
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.Administration.UI.CustomControls
{
    [GenerateTypedNameReferences]
    public partial class BwoinkPanel : BoxContainer
    {
        private readonly BwoinkSystem _bwoinkSystem;
        public readonly NetUserId ChannelId;

        public int Unread { get; private set; } = 0;

        public BwoinkPanel(BwoinkSystem bwoinkSys, NetUserId userId)
        {
            RobustXamlLoader.Load(this);
            _bwoinkSystem = bwoinkSys;
            ChannelId = userId;

            OnVisibilityChanged += c =>
            {
                if (c.Visible)
                    Unread = 0;
            };
            SenderLineEdit.OnTextEntered += Input_OnTextEntered;
        }

        private void Input_OnTextEntered(LineEdit.LineEditEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.Text))
                _bwoinkSystem.Send(ChannelId, args.Text);

            SenderLineEdit.Clear();
        }

        public void ReceiveLine(string text)
        {
            if (!Visible)
                Unread++;
            var formatted = new FormattedMessage(1);
            formatted.AddMarkup(text);
            TextOutput.AddMessage(formatted);
        }
    }
}
