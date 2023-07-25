using Content.Shared.Interaction.Events;
using Content.Shared.White.Cult;
using Robust.Server.GameObjects;
using Robust.Server.Player;

namespace Content.Server.White.Cult.Structures;

public sealed class CultStructureCraftSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RunicMetalComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, RunicMetalComponent component, UseInHandEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
            return;

        if (!_playerManager.TryGetSessionByEntity(args.User, out var session) || session is not IPlayerSession playerSession)
            return;

        if (component.UserInterface != null)
        {
            _uiSystem.CloseUi(component.UserInterface, playerSession);
            _uiSystem.OpenUi(component.UserInterface, playerSession);
        }
    }
}
