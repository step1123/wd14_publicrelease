using Content.Server.GameTicking;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaRestartRoundCommand : IUtkaCommand
{
    [Dependency] private UtkaTCPWrapper _utkaSocket = default!;

    public string Name => "restart_round";
    public Type RequestMessageType => typeof(UtkaRestartRoundRequest);
    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        IoCManager.InjectDependencies(this);

        EntitySystem.Get<GameTicker>().RestartRound();

        var response = new UtkaRestartRoundResponse()
        {
            Restarted = true
        };

        _utkaSocket.SendMessageToAll(response);
    }
}
