using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.White.Reputation.Commands;

[AnyCommand]
public sealed class ShowReputationCommand : IConsoleCommand
{
    [Dependency] private readonly ReputationManager _repManager = default!;

    public string Command => "showreput";
    public string Description => "Узнать свою репутацию.";
    public string Help => "Использование: showreput";
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        IoCManager.InjectDependencies(this);

        if (shell.Player == null)
            return;

        var value = await _repManager.GetPlayerReputation(shell.Player.UserId);
        if (value == null)
        {
            shell.WriteLine("Не удалось получить данные о репутации. Обратитесь к кодерам или попробуйте ещё раз.");
            return;
        }

        shell.WriteLine($"Ваша репутация: {value}");
    }
}
