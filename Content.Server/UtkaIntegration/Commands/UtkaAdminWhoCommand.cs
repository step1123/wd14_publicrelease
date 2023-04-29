using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Content.Server.Administration.Managers;
using Content.Shared.CCVar;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaAdminWhoCommand : IUtkaCommand
{
    public string Name => "adminwho";
    public Type RequestMessageType => typeof(UtkaAdminWhoRequest);

    [Dependency] private readonly UtkaTCPWrapper _utkaSocketWrapper = default!;

    public void Execute(UtkaTCPSession session, UtkaBaseMessage baseMessage)
    {
        if(baseMessage is not UtkaAdminWhoRequest message) return;
        IoCManager.InjectDependencies(this);

        var adminManager = IoCManager.Resolve<IAdminManager>();

        var admins = adminManager.ActiveAdmins.ToList();

        var adminsList = new List<string>();

        foreach (var admin in admins)
        {
            adminsList.Add(admin.Name);
        }

        var toUtkaMessage = new UtkaAdminWhoResponse()
        {
            Admins = adminsList
        };

        _utkaSocketWrapper.SendMessageToAll(toUtkaMessage);
    }
}
