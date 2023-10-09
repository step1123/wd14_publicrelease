using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.White.Reputation;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Server.White.Reputation;

public sealed class ReputationManager : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IServerNetManager _netMgr = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly Dictionary<NetUserId, ReputationInfo> _cacheReputation = new();
    private readonly Dictionary<NetUserId, DateTime> _playerConnectionTime = new();

    public override void Initialize()
    {
        base.Initialize();

        _netMgr.RegisterNetMessage<ReputationNetMsg>();

        _netMgr.Connecting += OnConnecting;
        _netMgr.Connected += OnConnected;

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeLocalEvent<UpdateCachedReputationEvent>(UpdateCachedReputation);
        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnPlayerSpawn);
    }

    #region Cache

        private void OnPlayerSpawn(PlayerBeforeSpawnEvent ev)
        {
            _playerConnectionTime.Add(ev.Player.UserId, DateTime.UtcNow);
        }

        private void OnConnected(object? sender, NetChannelArgs e)
        {
            _cacheReputation.TryGetValue(e.Channel.UserId, out var info);
            var msg = new ReputationNetMsg() { Info = info };
            _netMgr.ServerSendMessage(msg, e.Channel);
        }

        private async Task OnConnecting(NetConnectingArgs e)
        {
            var uid = e.UserId;
            var value = await GetPlayerReputation(uid);

            if (value == null)
                return;

            var info = new ReputationInfo() { Value = value.Value };
            _cacheReputation[e.UserId] = info;
        }

        private async void UpdateCachedReputation(UpdateCachedReputationEvent ev)
        {
            var player = ev.Player;
            if (!_cacheReputation.TryGetValue(player, out _))
                return;

            var value = await GetPlayerReputation(player);

            if (value == null)
                return;

            var info = new ReputationInfo() { Value = value.Value };
            _cacheReputation[player] = info;
        }

        private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
        {
            var connectedPlayers = _netMgr.Channels.Select(channel => channel.UserId).ToList();
            var newDictionary = _cacheReputation
                .Where(player => connectedPlayers.Contains(player.Key))
                .ToDictionary(player => player.Key, player => player.Value);

            _cacheReputation.Clear();
            _playerConnectionTime.Clear();

            foreach (var kvp in newDictionary)
            {
                _cacheReputation.Add(kvp.Key, kvp.Value);
            }
        }

    #endregion

    #region PublicApi

    public async void SetPlayerReputation(NetUserId player, float value)
    {
        var guid = player.UserId;
        await SetPlayerReputationTask(guid, value);
        RaiseLocalEvent(new UpdateCachedReputationEvent(player));
    }

    public async void ModifyPlayerReputation(NetUserId player, float value)
    {
        var guid = player.UserId;
        await ModifyPlayerReputationTask(guid, value);
        RaiseLocalEvent(new UpdateCachedReputationEvent(player));
    }

    public async Task<float?> GetPlayerReputation(NetUserId player)
    {
        var guid = player.UserId;
        return await GetPlayerReputationTask(guid);
    }

    public bool GetCachedPlayerReputation(NetUserId player, out float? value)
    {
        var success = _cacheReputation.TryGetValue(player, out var info);
        value = info?.Value;
        return success;
    }

    public bool GetCachedPlayerConnection(NetUserId player, out DateTime date)
    {
        var success = _playerConnectionTime.TryGetValue(player, out var dateTime);
        date = dateTime;
        return success;
    }

    public int GetPlayerWeight(float reputation)
    {
        return reputation switch
        {
            > 1000 => 9,
            > 700 => 8,
            > 500 => 7,
            > 300 => 6,
            > 100 => 5,
            > 50 => 4,
            > 15 => 3,
            < 0 => 1,
            _ => 2
        };
    }

    public IPlayerSession PickPlayerBasedOnReputation(List<IPlayerSession> prefList)
    {
        var list = new List<IPlayerSession>();

        foreach (var session in prefList)
        {
            if (!GetCachedPlayerReputation(session.UserId, out var value))
                continue;

            if (value == null)
                continue;

            var weight = GetPlayerWeight(value.Value);

            for (var i = 0; i < weight; i++)
            {
                list.Add(session);
            }
        }

        if (list.Count == 0)
            return _random.Pick(prefList);

        var number = _random.Next(list.Count - 1);
        return list[number];
    }

    #endregion

    #region Private

    private async Task SetPlayerReputationTask(Guid player, float value)
    {
        try
        {
            await _db.SetPlayerReputation(player, value);
        }
        catch (Exception)
        {
            // Nope
        }
    }

    private async Task ModifyPlayerReputationTask(Guid player, float value)
    {
        try
        {
            await _db.ModifyPlayerReputation(player, value);
        }
        catch (Exception)
        {
            // Nope
        }
    }

    private async Task<float?> GetPlayerReputationTask(Guid player)
    {
        try
        {
            return await _db.GetPlayerReputation(player);
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion
}
