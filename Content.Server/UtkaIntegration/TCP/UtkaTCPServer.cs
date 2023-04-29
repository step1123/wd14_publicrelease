using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared.CCVar;
using Content.Shared.White;
using NetCoreServer;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using YamlDotNet.Core.Tokens;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaTCPServer : TcpServer
{

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly ITimerManager _timerManager = default!;

    public static readonly Dictionary<string, IUtkaCommand> Commands = new();
    private List<UtkaTCPSession> _authenticatedSessions = new();

    private string? _key;

    protected override TcpSession CreateSession() { return new UtkaTCPSession(this); }
    public UtkaTCPServer(IPAddress address, int port) : base(address, port)
    {
        IoCManager.InjectDependencies(this);
        _cfg.OnValueChanged(WhiteCVars.UtkaSocketKey, key => _key = key, true);
        OptionKeepAlive = true;
    }

    public void SendMessageToAll(UtkaBaseMessage message)
    {
        foreach (var session in Sessions.Values.Cast<UtkaTCPSession>())
        {
            if(!session.Authenticated) continue;

            session.SendAsync(JsonSerializer.Serialize(message, message.GetType()));
        }
    }

    public void SendMessageToClient(UtkaTCPSession session, UtkaBaseMessage message)
    {
        session.SendAsync(JsonSerializer.Serialize(message, message.GetType()));
    }

    protected override void OnConnected(TcpSession session)
    {
        var utkaSession = (UtkaTCPSession) session;
        var cancellationToken = new CancellationTokenSource();

        utkaSession.OnMessageReceived += (sender, message) =>
        {
            ExecuteCommand(utkaSession, message);
        };

        var autoDisconnectionTimer = new Timer(25000, false, () =>
        {
            if (!utkaSession.Authenticated)
            {
                utkaSession.Disconnect();
            }
        });

        _timerManager.AddTimer(autoDisconnectionTimer, cancellationToken.Token);
    }

    protected override void OnDisconnecting(TcpSession session)
    {
        _authenticatedSessions.Remove((session as UtkaTCPSession)!);
        base.OnDisconnecting(session);
    }

    protected override void OnError(SocketError error)
    {
    }
    private void ExecuteCommand(UtkaTCPSession session, UtkaBaseMessage fromUtkaMessage)
    {
        var command = fromUtkaMessage.Command!;

        if (!Commands.ContainsKey(command))
        {
            return;
        }

        _taskManager.RunOnMainThread(() => Commands[command].Execute(session, fromUtkaMessage));
    }

    public static void RegisterCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        var commands = types.Where(type => typeof(IUtkaCommand).IsAssignableFrom(type) && type.GetInterfaces().Contains(typeof(IUtkaCommand))).ToList();

        foreach (var command in commands)
        {
            if (Activator.CreateInstance(command) is IUtkaCommand utkaCommand)
            {
                Commands[utkaCommand.Name] = utkaCommand;
            }
        }
    }
}
