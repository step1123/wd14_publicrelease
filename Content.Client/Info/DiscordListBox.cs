using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Info
{
    public sealed class DiscordListBox : BoxContainer
    {

        private IUriOpener _uriOpener;

        public DiscordListBox()
        {
            _uriOpener = IoCManager.Resolve<IUriOpener>();
            Orientation = LayoutOrientation.Vertical;
            AddDiscordServers();
        }

        private void AddDiscordServers()
        {
            AddDiscordServerInfo("Атараксия", "Проект для заинтересованных в отыгрыше.");
            AddDiscordServerInfo("Гласио", "Ролевой сервер с повышенным уровнем отыгрыша и собственным лором");
            AddDiscordServerInfo("Амур", "Проект с ЕРП направленностью.");
        }

        private void AddDiscordServerInfo(string serverName, string description)
        {
            var serverBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
            };

            var nameAndDescriptionBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
            };

            var serverNameLabel = new Label
            {
                Text = serverName,
                MinWidth = 200
            };

            var descriptionLabel = new RichTextLabel
            {
                MaxWidth = 500
            };
            descriptionLabel.SetMessage(FormattedMessage.FromMarkup(description));

            var buttonBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                HorizontalAlignment = HAlignment.Right
            };

            var connectButton = new Button
            {
                Text = "Discord"
            };

            connectButton.OnPressed += _ =>
            {
                OpenUrl(serverName, connectButton);
            };

            buttonBox.AddChild(connectButton);

            nameAndDescriptionBox.AddChild(serverNameLabel);
            nameAndDescriptionBox.AddChild(descriptionLabel);

            serverBox.AddChild(nameAndDescriptionBox);
            serverBox.AddChild(buttonBox);

            AddChild(serverBox);
        }

        private void OpenUrl(string serverName, Button button)
        {
            button.Disabled = true;

            var url = "";

            switch (serverName)
            {
                case "Атараксия":
                    url = "https://discord.gg/BCPkP3TcDT";
                    break;
                case "Гласио":
                    url = "https://discord.gg/TGzZep96cR";
                    break;
                case "Амур":
                    url = "https://discord.gg/vxHPsZ3Qyr";
                    break;
            }
            _uriOpener.OpenUri(url);

            button.Disabled = false;
        }
    }
}
