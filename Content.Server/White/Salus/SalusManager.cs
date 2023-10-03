using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.White;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Server.White;

public sealed class SalusManager
{
    [Dependency] private readonly IServerNetManager _netMgr = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private readonly HttpClient _httpClient = new();

    private bool _autoKickVpnUsers;
    private string _salusApiLink = default!;

    public void Initialize()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(2.5);
        _cfg.OnValueChanged(WhiteCVars.AutoKickVpnUsers, newValue => _autoKickVpnUsers = newValue, true);
        _cfg.OnValueChanged(WhiteCVars.SalusApiLink, newValue => _salusApiLink = newValue, true);

        _netMgr.Connecting += OnConnecting;
    }

    private async Task OnConnecting(NetConnectingArgs arg)
    {
        var ip = arg.IP.Address.ToString();
        bool usingVpn;

        try
        {
            var response = await _httpClient.GetAsync($"{_salusApiLink}{ip}");
            if (response.StatusCode != HttpStatusCode.OK) return;
            usingVpn = bool.Parse(await response.Content.ReadAsStringAsync());

        }
        catch (Exception e)
        {
            return;
        }

        if (usingVpn)
        {
            var logMessage = Loc.GetString("vpn-user-detected", ("user", arg.UserName), ("ip", ip));
            _adminLogger.Add(LogType.Unknown, LogImpact.Extreme, $"{logMessage}");

            if (_autoKickVpnUsers)
            {
                arg.Deny(Loc.GetString("vpn-user-disconnect-message"));
            }
        }
    }
}
