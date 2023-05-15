using Content.Server.Administration.Managers;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaJobBanCommand : IUtkaCommand
{
    public string Name => "jobban";
    public Type RequestMessageType => typeof(UtkaJobBanRequest);
    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if (baseMessage is not UtkaJobBanRequest message) return;

        var target = message.Ckey!;
        var job = message.Type!;
        var reason = message.Reason!;
        var minutes = (uint) message.Duration!;
        var isGlobalBan = (bool) message.Global!;
        var admin = message.ACkey!;

        IoCManager.Resolve<RoleBanManager>().UtkaCreateJobBan(admin, target, job, reason, minutes, isGlobalBan);
    }
}
