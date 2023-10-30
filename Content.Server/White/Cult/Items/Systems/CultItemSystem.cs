using Content.Server.White.Cult.Items.Components;
using Content.Shared.Ghost;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.White.Cult;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server.White.Cult.Items.Systems;

public sealed class CultItemSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultItemComponent, ContainerGettingInsertedAttemptEvent>(OnHandPickUp);
    }

    private void OnHandPickUp(EntityUid uid, CultItemComponent component, ContainerGettingInsertedAttemptEvent args)
    {
        var user = args.Container.Owner;
        if (!HasComp<HandsComponent>(user) || HasComp<CultistComponent>(user) || HasComp<SharedGhostComponent>(user))
            return;

        args.Cancel();
        _transform.SetCoordinates(uid, Transform(user).Coordinates);
        _transform.AttachToGridOrMap(uid);
        _throwing.TryThrow(uid, _random.NextVector2());
        _popupSystem.PopupEntity(Loc.GetString("cult-item-component-not-cultist", ("name", Name(uid))),
            user, user);
    }
}
