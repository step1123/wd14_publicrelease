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
using System.Text.RegularExpressions;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaTCPSession : TcpSession
{
    public event EventHandler<UtkaBaseMessage>? OnMessageReceived;
    public bool Authenticated { get; set; }
    private string BufferCahce = string.Empty;


    public UtkaTCPSession(TcpServer server) : base(server)
    {
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        BufferCahce += Encoding.UTF8.GetString(buffer, (int) offset, (int) size);

        HandleCache();
    }

    protected override void OnError(SocketError error)
    {
        SendAsync($"{error.ToString()}");
        base.OnError(error);
    }

    protected override void OnConnected()
    {
        SendAsync("Utka sosal handshake");
        base.OnConnected();
    }

    private bool ValidateMessage(string message, out UtkaBaseMessage? fromDiscordMessage)
    {
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
        BufferCahce = string.Empty;
    }

    private void HandleCache()
    {
        var regex = new Regex("{.+?}");
        var matches = regex.Matches(BufferCahce);

        foreach (Match match in matches)
        {
            var pos = BufferCahce.IndexOf(match.Value);
            BufferCahce = BufferCahce.Substring(0, pos) + BufferCahce.Substring(pos + match.Value.Length);

            if (!ValidateMessage(match.Value, out var message))
            {
                this.SendAsync("Validation fail");
                return;
            }

            OnMessageReceived?.Invoke(this, message!);
        }
    }
}
