using System.Net;

namespace Content.Server.UtkaIntegration;

public interface IUtkaCommand
{
    string Name { get; }
    Type RequestMessageType { get; }
    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage);
}
