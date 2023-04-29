using System.Linq;
using System.Net;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Robust.Shared.Utility;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaAssayCommand : IUtkaCommand
{
    public string Name => "asay";
    public Type RequestMessageType => typeof(UtkaAsayRequest);

    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if(baseMessage is not UtkaAsayRequest message) return;

        var ckey = message.ACkey;

        if(string.IsNullOrWhiteSpace(message.Message) || string.IsNullOrWhiteSpace(ckey)) return;

        var chatManager = IoCManager.Resolve<IChatManager>();

        chatManager.SendHookAdminChat(ckey, message.Message);

    }
}
