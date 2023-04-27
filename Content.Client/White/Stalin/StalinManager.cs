using Content.Shared.White.SaltedYayca;
using Robust.Client.UserInterface;
using Robust.Shared.Network;

namespace Content.Client.White.Stalin;

public sealed class StalinManager
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IUriOpener _uriOpener = default!;

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
        _uriOpener.OpenUri(message.Uri);
    }
}
