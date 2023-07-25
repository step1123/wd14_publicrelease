using Content.Server.EUI;
using Content.Server.Popups;
using Content.Server.White.Cult.Runes.Comps;
using Content.Shared.Actions;
using Content.Shared.Eui;
using Content.Shared.Popups;
using Content.Shared.White.Cult.UI;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server.White.Cult.UI;

public sealed class TeleportSpellEui : BaseEui
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    private SharedTransformSystem _transformSystem;
    private PopupSystem _popupSystem;


    private EntityUid _performer;
    private EntityUid _target;

    private EntityCoordinates _initialOwnerCoords;
    private EntityCoordinates _initialTargetCoords;

    private bool _used;


    public TeleportSpellEui(EntityUid performer, EntityUid target)
    {
        IoCManager.InjectDependencies(this);

        _transformSystem = _entityManager.System<SharedTransformSystem>();
        _popupSystem = _entityManager.System<PopupSystem>();

        _performer = performer;
        _target = target;

        _initialOwnerCoords = _entityManager.GetComponent<TransformComponent>(_performer).Coordinates;
        _initialTargetCoords = _entityManager.GetComponent<TransformComponent>(_target).Coordinates;

        Timer.Spawn(TimeSpan.FromSeconds(10), Close );
    }

    public override EuiStateBase GetNewState()
    {
        var runes = _entityManager.EntityQuery<CultRuneTeleportComponent>();
        var state = new TeleportSpellEuiState();

        foreach (var rune in runes)
        {
            state.Runes.Add((int)rune.Owner, rune.Label!);
        }

        return state;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if(_used) return;

        if (msg is not TeleportSpellTargetRuneSelected cast)
        {
            return;
        }

        var performerPosition = _entityManager.GetComponent<TransformComponent>(_performer).Coordinates;
        var targetPosition = _entityManager.GetComponent<TransformComponent>(_target).Coordinates;;

        performerPosition.TryDistance(_entityManager, targetPosition, out var distance);

        if(distance > 1.5f)
        {
            _popupSystem.PopupEntity("Too far", _performer, PopupType.Medium);
            return;
        }

        TransformComponent? runeTransform = null!;

        foreach (var runeComponent in _entityManager.EntityQuery<CultRuneTeleportComponent>())
        {
            if (runeComponent.Owner == new EntityUid(cast.RuneUid))
            {
                runeTransform = _entityManager.GetComponent<TransformComponent>(runeComponent.Owner);
            }
        }

        if (runeTransform is null)
        {
            _popupSystem.PopupEntity("Rune is gone", _performer);
            DoStateUpdate();
            return;
        }

        _used = true;

        _transformSystem.SetCoordinates(_target, runeTransform.Coordinates);
        Close();
    }
}
