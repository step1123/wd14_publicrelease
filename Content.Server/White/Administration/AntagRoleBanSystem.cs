using Content.Server.Administration.Managers;
using Robust.Server.Player;

namespace Content.Server.White.Administration;

public sealed class AntagRoleBanSystem : EntitySystem
{
    [Dependency] private readonly RoleBanManager _roleBan = default!;

    public bool HasAntagBan(IPlayerSession player)
    {
        var antagBan = _roleBan.GetRoleBans(player.UserId)?.TryGetValue("Job:AntagRole",out _);
        return antagBan != null && antagBan.Value;
    }
}
