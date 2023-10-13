using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.White.JobWhitelist;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Enums;
using Robust.Shared.Network;

namespace Content.Server.White.JobWhitelist;

public sealed class JobWhitelistManager
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private  ISawmill _sawmill = default!;

    private readonly Dictionary<NetUserId, List<string>> _cachedAllowedJobs = new();

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("jobwhitelist");
        _netManager.RegisterNetMessage<MsgJobWL>();
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus != SessionStatus.Connected)
            return;

        await CacheDbAllowedJobsAsync(e.Session);
    }

    private async Task CacheDbAllowedJobsAsync(IPlayerSession pSession)
    {
        var allowedJobs = await _db.TakeAllowedJobWhitelistAsync(pSession.UserId);
        _cachedAllowedJobs[pSession.UserId] = allowedJobs ?? new();
        SendAllowedJobs(pSession);
    }

    public void SendAllowedJobs(LocatedPlayerData located)
    {
        if (!_playerManager.TryGetSessionById(located.UserId, out var player))
            return;

        SendAllowedJobs(player);
    }

    public void SendAllowedJobs(IPlayerSession pSession)
    {
        if (!_cachedAllowedJobs.TryGetValue(pSession.UserId, out var allowedJobs))
        {
            _sawmill.Error("jobwhitelist", $"Tried to send jobwhitelist for {pSession.Name} but none cached?");
            return;
        }

        var msg = new MsgJobWL()
        {
            AllowedJobs = allowedJobs ?? new()
        };

        _sawmill.Debug($"Sent jobwhitelist to {pSession.Name}");
        _netManager.ServerSendMessage(msg, pSession.ConnectedClient);
    }

    public async void AddToWhitelist(NetUserId player, string job)
    {
        if (_cachedAllowedJobs[player].Contains(job))
            return;

        await _db.AddToJobWhitelistAsync(player, job);

        if (!_playerManager.TryGetSessionById(player, out var pSession))
            return;

        await CacheDbAllowedJobsAsync(pSession);
    }

    public async void RemoveFromWhitelist(NetUserId player, string? job)
    {
        await _db.RemoveFromJobWhitelistAsync(player, job);

        if (!_playerManager.TryGetSessionById(player, out var pSession))
            return;

        await CacheDbAllowedJobsAsync(pSession);
    }

    public List<string> TakeAllowedJobs(NetUserId player)
    {
        return _cachedAllowedJobs[player];
    }

    public bool IsAllowed(NetUserId player, string job)
    {
        return _cachedAllowedJobs[player].Contains(job);
    }
}
