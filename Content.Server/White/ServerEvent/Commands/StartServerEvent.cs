using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.White.ServerEvent;
using Robust.Shared.Console;

namespace Content.Server.White.ServerEvent.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class StartServerEvent : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entities = default!;

    public string Command => "startevent";
    public string Description => "starting a new event";
    public string Help => "startevent <event>";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteError("Not enough args!");
        }

        if(!_entities.System<ServerEventSystem>().TryStartEvent(args[0]))
            shell.WriteError("Oopsie! error on starting event!");
        else
            shell.WriteLine("Done!");

    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<ServerEventPrototype>(),
            "<serverEventPrototype>");
    }
}
