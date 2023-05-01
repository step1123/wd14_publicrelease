using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;

namespace Content.Server.White.Other.Lazy;

public sealed class EarsSpawnSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EarsSpawnComponent, GetVerbsEvent<AlternativeVerb>>(AddSummonVerb);
        SubscribeLocalEvent<EarsSpawnComponent, GetItemActionsEvent>(GetSummonAction);
        SubscribeLocalEvent<EarsSpawnComponent, SummonActionEarsEvent>(OnSummon);
    }

    private const string Ears = "ClothingHeadHatCatEars";
    private const string UserNeededKey = "merkkaa";

    private void AddSummonVerb(EntityUid uid, EarsSpawnComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                AttemptSummon(component, args.User);
            },
            Text = Loc.GetString("summon cat ears"),
            Priority = 2
        };

        args.Verbs.Add(verb);
    }

    private void OnSummon(EntityUid uid, EarsSpawnComponent component, SummonActionEarsEvent args)
    {
        AttemptSummon(component, args.Performer);
    }

    private void AttemptSummon(EarsSpawnComponent component, EntityUid user)
    {
        if (!_blocker.CanInteract(user, component.Owner))
            return;

        if (TryComp<ActorComponent>(user, out var actorComponent))
        {
            var userKey = actorComponent.PlayerSession.Name;
            if (userKey != UserNeededKey)
            {
                _popupSystem.PopupEntity("Вы не являетесь потомком кошко-богини.", user, PopupType.Medium);
                return;
            }
        }

        SpawnEars(user);
    }

    private void SpawnEars(EntityUid player)
    {
        var transform = CompOrNull<TransformComponent>(player)?.Coordinates;

        if (transform == null)
            return;

        var ears = _entityManager.SpawnEntity(Ears, transform.Value);
        _handsSystem.PickupOrDrop(player, ears);
    }

    private static void GetSummonAction(EntityUid uid, EarsSpawnComponent component, GetItemActionsEvent args)
    {
        args.Actions.Add(component.SummonAction);
    }
}

public sealed class SummonActionEarsEvent : InstantActionEvent
{
}
