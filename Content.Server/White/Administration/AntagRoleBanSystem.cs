using Content.Server.Administration.Managers;
using Robust.Server.Player;
using Robust.Shared.Network;

namespace Content.Server.White.Administration;

public sealed class AntagRoleBanSystem : EntitySystem
{
    [Dependency] private readonly RoleBanManager _roleBan = default!;

    public bool HasAntagBan(NetUserId id)
    {
        var antagBan = _roleBan.GetRoleBans(id)?.TryGetValue("Job:AntagRole",out _);
        return antagBan != null && antagBan.Value;
    }
}
