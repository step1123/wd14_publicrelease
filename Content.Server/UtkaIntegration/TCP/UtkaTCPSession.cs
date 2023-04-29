using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using Content.Shared.CCVar;
using NetCoreServer;
using Newtonsoft.Json.Linq;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaTCPSession : TcpSession
{
    public event EventHandler<UtkaBaseMessage>? OnMessageReceived;
    public bool Authenticated { get; set; }


    public UtkaTCPSession(TcpServer server) : base(server)
    {
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        if (!ValidateMessage(buffer, offset, size, out var message))
        {
            this.SendAsync("Validation fail");
            return;
        }

        OnMessageReceived?.Invoke(this, message!);
    }

    protected override void OnError(SocketError error)
    {
        SendAsync($"{error.ToString()}");
        base.OnError(error);
    }

    protected override void OnConnected()
    {
        SendAsync("Hello from грабли, знай утка я ебал тебя в зад!!!");
        base.OnConnected();
    }

    private bool ValidateMessage(byte[] buffer, long offset, long size, out UtkaBaseMessage? fromDiscordMessage)
    {
        var message = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
        fromDiscordMessage = null;

        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        var commandName = JObject.Parse(message)["command"];
        if (commandName == null) return false;

        var utkaCommand = UtkaTCPServer.Commands.Values.FirstOrDefault(x => x.Name == commandName.ToString());

        if (utkaCommand == null) return false;

        var messageType = utkaCommand.RequestMessageType;

        try
        {
            fromDiscordMessage = JsonSerializer.Deserialize(message, messageType) as UtkaBaseMessage;
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }
    protected override void OnDisconnected()
    {
        base.OnDisconnecting();
        Dispose();
    }
}
