using Content.Server.Administration;
using Content.Server.Database;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaUnJobBanCommand : IUtkaCommand
{
    [Dependency] private UtkaTCPWrapper _utkaSocketWrapper = default!;

    public string Name => "unjobban";
    public Type RequestMessageType => typeof(UtkaUnJobBanRequest);
    public async void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if (baseMessage is not UtkaUnJobBanRequest message) return;

        var dbMan = IoCManager.Resolve<IServerDbManager>();
        var locator = IoCManager.Resolve<IPlayerLocator>();
        IoCManager.InjectDependencies(this);

        var located = await locator.LookupIdByNameOrIdAsync(message.ACkey!);
        if (located == null)
        {
            UtkaSendResponse(false);
            return;
        }

        var player = located.UserId;

        var ban = await dbMan.GetServerRoleBanAsync(message.Bid!.Value);
        if (ban == null || ban.Unban != null)
        {
            UtkaSendResponse(false);
            return;
        }

        var adminData = await dbMan.GetAdminDataForAsync(player);
        if (adminData?.AdminRank == null || ban.ServerName != "unknown" && adminData.AdminServer is not (null or "unknown") && adminData.AdminServer != ban.ServerName)
        {
            UtkaSendResponse(false);
            return;
        }

        await dbMan.AddServerRoleUnbanAsync(new ServerRoleUnbanDef(message.Bid!.Value, player, DateTimeOffset.Now));

        UtkaSendResponse(true);
    }

    private void UtkaSendResponse(bool unbanned)
    {
        var utkaResponse = new UtkaUnJobBanResponse()
        {
            Unbanned = unbanned
        };

        _utkaSocketWrapper.SendMessageToAll(utkaResponse);
    }
}
