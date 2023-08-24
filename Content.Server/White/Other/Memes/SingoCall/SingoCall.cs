using Content.Server.Chat.Managers;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.White.Cult.UI;
using Robust.Server.GameObjects;

namespace Content.Server.White.Other.Memes.SingoCall;

public sealed class SingoCall : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SingoCallComponent, InteractUsingEvent>(Combine);
        SubscribeLocalEvent<HumanoidAppearanceComponent, SinguloCallMessage>(OnChoosed);
    }

    private void Combine(EntityUid uid, SingoCallComponent _, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<SingoCallComponent>(args.Target))
            return;

        if (!HasComp<SingoCallComponent>(args.Used))
            return;

        if (!TryComp<ActorComponent>(args.User, out var actorComponent))
            return;

        if (!_ui.TryGetUi(args.User, SinguloCallUIKey.Key, out var bui))
            return;

        _ui.OpenUi(bui, actorComponent.PlayerSession);
    }

    private void OnChoosed(EntityUid uid, HumanoidAppearanceComponent component, SinguloCallMessage message)
    {
        if (!TryComp<ActorComponent>(message.Entity, out var actorComponent))
            return;

        if (message.Name != "Я уверен")
            return;

        var ckey = actorComponent.PlayerSession.Name;

        SpawnSingo(message.Entity);

        _chatManager.SendAdminAnnouncement($"{ckey} создал сингулярность с помощью Bag of Holding");
    }

    private void SpawnSingo(EntityUid player)
    {
        var transform = CompOrNull<TransformComponent>(player)?.Coordinates;

        if (transform == null)
            return;

        EntityManager.SpawnEntity("Singularity", transform.Value);
    }
}
