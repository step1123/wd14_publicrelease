using Content.Server.Administration;
using Content.Server.White.Administration;
using Content.Shared.Administration;
using Content.Shared.White.Administration;
using Robust.Shared.Console;

namespace Content.Server.White.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class InvisibilityCommand : IConsoleCommand
{
    public string Command => "invisibility";
    public string Description => "Переключает режим невидимости.";
    public string Help => "invisibility";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player == null)
            shell.WriteLine("You can only toggle invisibility on a client.");

        var entityManager = IoCManager.Resolve<EntityManager>();
        var uid = shell.Player?.AttachedEntity;
        if (uid == null
            || !entityManager.TryGetComponent<InvisibilityComponent>(uid, out var invisibilityComponent))
            return;

        entityManager.System<InvisibilitySystem>().ToggleInvisibility(uid.Value, invisibilityComponent);
    }
}
