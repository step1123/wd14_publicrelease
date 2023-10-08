using System.Diagnostics.CodeAnalysis;
using Content.Shared.White.Reputation;
using Robust.Shared.Network;

namespace Content.Client.White.Reputation;

public sealed class ReputationManager
{
    [Dependency] private readonly IClientNetManager _netMgr = default!;

    private ReputationInfo? _info;

    public void Initialize()
    {
        _netMgr.RegisterNetMessage<ReputationNetMsg>(msg => _info = msg.Info);
    }

    public bool TryGetInfo([NotNullWhen(true)] out float? value)
    {
        value = _info?.Value;
        return _info != null;
    }
}
