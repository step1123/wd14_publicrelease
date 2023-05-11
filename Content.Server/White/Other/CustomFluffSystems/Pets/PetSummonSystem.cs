using Content.Server.Ghost.Roles.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;

namespace Content.Server.White.Other.CustomFluffSystems.Pets;

public sealed class PetSummonSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private IReadOnlyDictionary<string, string> MobMap = new Dictionary<string, string>()
    {
        { "Wanderer_", "KommandantPetSpider" },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PetSummonComponent, GetItemActionsEvent>(GetSummonAction);
        SubscribeLocalEvent<PetSummonComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<PetSummonComponent, GetVerbsEvent<AlternativeVerb>>(AddSummonVerb);
        SubscribeLocalEvent<PetSummonComponent, PetSummonActionEvent>(OnSummon);
        SubscribeLocalEvent<PetSummonComponent, PetGhostSummonActionEvent>(OnGhostSummon);
    }

    private void OnGhostSummon(EntityUid uid, PetSummonComponent component, PetGhostSummonActionEvent args)
    {
        AttemptSummon(component, args.Performer, true);
    }

    private void OnSummon(EntityUid uid, PetSummonComponent component, PetSummonActionEvent args)
    {
        AttemptSummon(component, args.Performer, false);
    }

    private void AddSummonVerb(EntityUid uid, PetSummonComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                AttemptSummon(component, args.User, false);
            },
            Text = "Призвать питомца",
            Priority = 2
        };

        AlternativeVerb ghostVerb = new()
        {
            Act = () =>
            {
                AttemptSummon(component, args.User, true);
            },
            Text = "Призвать питомца-призрак",
            Priority = 2
        };

        args.Verbs.Add(verb);
        args.Verbs.Add(ghostVerb);
    }

    private void OnExamine(EntityUid uid, PetSummonComponent component, ExaminedEvent args)
    {
        args.PushMarkup($"Осталось призывов: {component.UsesLeft}");
    }

    private void AttemptSummon(PetSummonComponent component, EntityUid user, bool ghostRole)
    {
        if (!_blocker.CanInteract(user, component.Owner))
            return;

        string? mobProto = null;
        if (TryComp<ActorComponent>(user, out var actorComponent))
        {
            var userKey = actorComponent.PlayerSession.Name;

            if (!MobMap.TryGetValue(userKey, out var proto))
            {
                _popupSystem.PopupEntity("Вы не достойны", user, PopupType.Medium);
                return;
            }

            mobProto = proto;
        }

        if (component.UsesLeft == 0)
        {
            _popupSystem.PopupEntity("Больше нет зарядов!", user, PopupType.Medium);
            return;
        }

        if (component.SummonedEntity != null)
        {
            if (!TryComp<MobStateComponent>(component.SummonedEntity, out var mobState))
            {
                component.SummonedEntity = null;
            }
            else
            {
                if (mobState.CurrentState is MobState.Dead or MobState.Invalid)
                    component.SummonedEntity = null;
                else
                {
                    _popupSystem.PopupEntity("Ваш питомец уже призван", user, PopupType.Medium);
                    return;
                }
            }

        }

        if (mobProto != null)
            SummonPet(user, component, mobProto, ghostRole);
    }

    private void SummonPet(EntityUid user, PetSummonComponent component, string mobProto, bool ghostRole)
    {
        var transform = CompOrNull<TransformComponent>(user)?.Coordinates;

        if (transform == null)
            return;

        var entity = _entityManager.SpawnEntity(mobProto, transform.Value);
        component.UsesLeft--;
        component.SummonedEntity = entity;

        if (ghostRole)
            SetupGhostRole(entity, user);
        else
            RemComp<GhostRoleComponent>(entity);
    }

    private void SetupGhostRole(EntityUid entity, EntityUid user)
    {
        EnsureComp<GhostTakeoverAvailableComponent>(entity);

        if (!TryComp<GhostRoleComponent>(entity, out var ghostRole))
            return;

        var meta = MetaData(user);
        ghostRole.RoleName = $"Питомец {meta.EntityName}";
        ghostRole.RoleDescription = $"Следуйте за хозяином - {meta.EntityName} и выполняйте его приказы";
        ghostRole.RoleRules = $"Вы должны до самого конца следовать за своим хозяином - {meta.EntityName} и послушно выполнять его приказы, иначе можете быть уничтожены.";
    }

    private void GetSummonAction(EntityUid uid, PetSummonComponent component, GetItemActionsEvent args)
    {
        args.Actions.Add(component.PetSummonAction);
        args.Actions.Add(component.PetGhostSummonAction);
    }
}

public sealed class PetSummonActionEvent : InstantActionEvent
{
}

public sealed class PetGhostSummonActionEvent : InstantActionEvent
{
}
