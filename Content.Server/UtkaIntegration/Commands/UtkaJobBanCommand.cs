using Content.Server.Administration.Managers;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaJobBanCommand : IUtkaCommand
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Name => "jobban";
    public Type RequestMessageType => typeof(UtkaJobBanRequest);
    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        IoCManager.InjectDependencies(this);

        if (baseMessage is not UtkaJobBanRequest message) return;
        var target = message.Ckey!;
        var job = message.Type!;
        var reason = message.Reason!;
        var minutes = (uint) message.Duration!;
        var isGlobalBan = (bool) message.Global!;
        var admin = message.ACkey!;

        var roleBanManager = IoCManager.Resolve<RoleBanManager>();

        if (_prototypeManager.TryIndex<DepartmentPrototype>(job, out var departmentProto))
            roleBanManager.UtkaCreateDepartmentBan(admin, target, departmentProto, reason, minutes, isGlobalBan);

        else
            roleBanManager.UtkaCreateJobBan(admin, target, job, reason, minutes, isGlobalBan);
    }
}


