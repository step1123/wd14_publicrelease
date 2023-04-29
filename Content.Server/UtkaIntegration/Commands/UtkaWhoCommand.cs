using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaWhoCommand : IUtkaCommand
{
    public string Name => "who";
    public Type RequestMessageType => typeof(UtkaWhoRequest);
    
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private UtkaTCPWrapper _utkaSocketWrapper = default!;

    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if(baseMessage is not UtkaWhoRequest _) return;

        IoCManager.InjectDependencies(this);

        var players = Filter.GetAllPlayers().ToList();
        var playerNames = players
            .Where(player => player.Status != SessionStatus.Disconnected)
            .Select(x => x.Name);

        var toUtkaMessage = new UtkaWhoResponse()
        {
            Players = playerNames.ToList()
        };

        _utkaSocketWrapper.SendMessageToAll(toUtkaMessage);
    }
}
