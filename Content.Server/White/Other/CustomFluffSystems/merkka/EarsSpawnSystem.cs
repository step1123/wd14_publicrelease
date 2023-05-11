using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;

namespace Content.Server.White.Other.CustomFluffSystems.merkka;

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
        SubscribeLocalEvent<EarsSpawnComponent, SummonActionCatEvent>(OnSummonCat);
        SubscribeLocalEvent<EarsSpawnComponent, ExaminedEvent>(OnExamined);
    }

    private const string Ears = "ClothingHeadHatCatEars";
    private const string Cat = "MobCatMurka";
    private const string UserNeededKey = "merkkaa";

    private void OnExamined(EntityUid u, EarsSpawnComponent comp, ExaminedEvent ev)
    {
        ev.PushMarkup($"Зарядов для ушей: {comp.CatEarsUses}\n" +
                      $"Зарядов для создания кота: {comp.СatSpawnUses}");
    }

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

        AlternativeVerb verbCat = new()
        {
            Act = () =>
            {
                AttemptSummonCat(component, args.User);
            },
            Text = Loc.GetString("summon cat"),
            Priority = 3
        };

        args.Verbs.Add(verb);
        args.Verbs.Add(verbCat);
    }

    private void OnSummon(EntityUid uid, EarsSpawnComponent component, SummonActionEarsEvent args)
    {
        AttemptSummon(component, args.Performer);
    }

    private void OnSummonCat(EntityUid uid, EarsSpawnComponent component, SummonActionCatEvent args)
    {
        AttemptSummonCat(component, args.Performer);
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

        if (component.CatEarsUses == 0)
        {
            _popupSystem.PopupEntity("Больше нет зарядов!", user, PopupType.Medium);
            return;
        }

        SpawnEars(user, component);
    }

    private void AttemptSummonCat(EarsSpawnComponent component, EntityUid user)
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

        if (component.СatSpawnUses == 0)
        {
            _popupSystem.PopupEntity("Больше нет зарядов!", user, PopupType.Medium);
            return;
        }

        SpawnCat(user, component);
    }

    private void SpawnEars(EntityUid player, EarsSpawnComponent comp)
    {
        var transform = CompOrNull<TransformComponent>(player)?.Coordinates;

        if (transform == null)
            return;

        var ears = _entityManager.SpawnEntity(Ears, transform.Value);
        _handsSystem.PickupOrDrop(player, ears);
        comp.CatEarsUses--;
    }

    private void SpawnCat(EntityUid player, EarsSpawnComponent comp)
    {
        var transform = CompOrNull<TransformComponent>(player)?.Coordinates;

        if (transform == null)
            return;

        _entityManager.SpawnEntity(Cat, transform.Value);
        comp.СatSpawnUses--;
    }

    private static void GetSummonAction(EntityUid uid, EarsSpawnComponent component, GetItemActionsEvent args)
    {
        args.Actions.Add(component.SummonAction);
        args.Actions.Add(component.SummonActionCat);
    }
}

public sealed class SummonActionEarsEvent : InstantActionEvent
{
}

public sealed class SummonActionCatEvent : InstantActionEvent
{
}
