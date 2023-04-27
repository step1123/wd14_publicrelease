using System.Net;
using Content.Server.Chat.Managers;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaSendOOCMessage : IUtkaCommand
{
    public string Name => "ooc";
    public Type RequestMessageType => typeof(UtkaOOCRequest);

    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if (baseMessage is not UtkaOOCRequest message) return;
        if(string.IsNullOrWhiteSpace(message.Message) || string.IsNullOrWhiteSpace(message.CKey)) return;


        var chatSystem = IoCManager.Resolve<IChatManager>();
        chatSystem.SendHookOOC($"{message.CKey}", $"{message.Message}");
    }
}
