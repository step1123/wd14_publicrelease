using Content.Client.UserInterface.Controls;
using Content.Client.White.Stalin.StalinUi;
using Content.Shared.White.SaltedYayca;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.White.Stalin;

public sealed class StalinManager
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IUriOpener _uriOpener = default!;
    private StalinLinkWindow _stalinLinkWindow = null!;

    public void Initialize()
    {
        _netManager.RegisterNetMessage<DiscordAuthResponse>(OnStalinResponse);
    }

    public void RequestUri()
    {
        _netManager.ClientSendMessage(new DiscordAuthRequest());
    }

    private void OnStalinResponse(DiscordAuthResponse message)
    {
        if (_stalinLinkWindow != null)
        {
            _stalinLinkWindow.Close();
        }
        _stalinLinkWindow = new StalinLinkWindow();
        _stalinLinkWindow.SetUri(message.Uri);
        _stalinLinkWindow.OpenCentered();
    }
}
