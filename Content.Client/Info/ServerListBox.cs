using Robust.Client;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Info
{
    public sealed class ServerListBox : BoxContainer
    {
        private IGameController _gameController;
        private List<Button> _connectButtons = new();

        public ServerListBox()
        {
            _gameController = IoCManager.Resolve<IGameController>();
            Orientation = LayoutOrientation.Vertical;
            AddServers();
        }

        private void AddServers()
        {
            AddServerInfo("Грид", "Сервер с постоянным безумием");
            AddServerInfo("Мэйд", "Сервер с лучшим сервисом");
            AddServerInfo("Енги", "Сервер с расслабленным геймплеем");
            AddServerInfo("Амур", "Сервер для любителей ЕРП");
            AddServerInfo("Инфинити️", "Сервер без правил");
            AddServerInfo("Гласио", "Вайтлистовый сервер");
            AddServerInfo("Атараксия", "Для любителей ролевой игры");
        }

        private void AddServerInfo(string serverName, string description)
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
                Text = "Connect"
            };

            _connectButtons.Add(connectButton);

            connectButton.OnPressed += _ =>
            {
                ConnectToServer(serverName);

                foreach (var connectButton in _connectButtons)
                {
                    connectButton.Disabled = true;
                }
            };

            buttonBox.AddChild(connectButton);

            nameAndDescriptionBox.AddChild(serverNameLabel);
            nameAndDescriptionBox.AddChild(descriptionLabel);

            serverBox.AddChild(nameAndDescriptionBox);
            serverBox.AddChild(buttonBox);

            AddChild(serverBox);
        }

        private void ConnectToServer(string serverName)
        {
            var url = "";

            switch (serverName)
            {
                case "Грид":
                    url = "ss14://s0.ss14.su:3333";
                    break;
                case "Мэйд":
                    url = "ss14://s5.ss14.su:6666";
                    break;
                case "Енги":
                    url = "ss14://s5.ss14.su:7777";
                    break;
                case "Амур":
                    url = "ss14://s0.ss14.su:8888";
                    break;
                case "Инфинити️":
                    url = "ss14://s0.ss14.su:5555";
                    break;
                case "Гласио":
                    url = "ss14://s0.ss14.su:4444";
                    break;
                case "Атараксия":
                    url = "ss14://s0.ss14.su:10101";
                    break;
            }
            _gameController.Redial(url, "Connecting to another server...");
        }
    }
}
