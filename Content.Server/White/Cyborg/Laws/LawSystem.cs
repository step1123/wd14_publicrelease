using Content.Server.Chat.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Database;
using Content.Shared.White.Cyborg.Laws;
using Content.Shared.White.Cyborg.Laws.Component;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.Laws;

public sealed class LawsSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LawsComponent, StateLawsMessage>(OnStateLaws);
        SubscribeLocalEvent<LawsComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LawsComponent, BoundUIOpenedEvent>(OnOpenUi);
    }

    private void OnOpenUi(EntityUid uid, LawsComponent component, BoundUIOpenedEvent args)
    {
        if (args.UiKey is LawsUiKey)
            _alerts.ClearAlert(uid, AlertType.Law);
    }

    private void OnStateLaws(EntityUid uid, LawsComponent component, StateLawsMessage args)
    {
        StateLaws(uid, args.Laws);
    }

    private void OnPlayerAttached(EntityUid uid, LawsComponent component, PlayerAttachedEvent args)
    {
        if (!_ui.TryGetUi(uid, LawsUiKey.Key, out var ui))
            return;

        _ui.TryOpen(uid, LawsUiKey.Key, args.Player);
    }

    public void StateLaws(EntityUid uid, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        StateLaws(uid, component.Laws);
    }

    public void StateLaws(EntityUid uid, List<string> laws, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.CanState)
            return;

        if (component.StateTime != null && _timing.CurTime < component.StateTime)
            return;

        component.StateTime = _timing.CurTime + component.StateCD;

        foreach (var law in laws)
        {
            _chat.TrySendInGameICMessage(uid, law, InGameICChatType.Speak, false);
        }
    }


    public void UpdateUserInterface(EntityUid uid, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        if (!_ui.TryGetUi(uid, LawsUiKey.Key, out var ui))
            return;
        var state = new LawsUpdateState(component.Laws, uid);

        UserInterfaceSystem.SetUiState(ui, state);
    }

    private void PinLaws(EntityUid uid, LawsComponent component)
    {
        Dirty(component);
        UpdateUserInterface(uid, component);
        _alerts.ShowAlert(uid, AlertType.Law);
    }


    [PublicAPI]
    public void ReIndexLaw(EntityUid uid, int index, int newIndex, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        if (newIndex > component.Laws.Count || newIndex < 0)
            return;
        (component.Laws[index], component.Laws[newIndex]) = (component.Laws[newIndex], component.Laws[index]);

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"The order of {ToPrettyString(uid):player}'s law has been changed from {index} to " +
            $"{newIndex}. Law: {GetLaw(uid, index)}");

        PinLaws(uid, component);
    }

    public string? GetLaw(EntityUid uid, int index, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component) || index < 0 || index > component.Laws.Count)
            return null;

        return component.Laws[index];
    }

    public void ClearLaws(EntityUid uid, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        component.Laws.Clear();
        PinLaws(uid, component);
    }

    public void AddLaw(EntityUid uid, string law, int? index = null, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (index == null)
            index = component.Laws.Count;

        index = Math.Clamp((int) index, 0, component.Laws.Count);

        component.Laws.Insert((int) index, law);

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"The law '{law}' has been added with index {index} to {ToPrettyString(uid).Name}!");

        PinLaws(uid, component);
    }

    public void RemoveLaw(EntityUid uid, int? index = null, LawsComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (index == null)
            index = component.Laws.Count;

        if (component.Laws.Count == 0)
            return;

        index = Math.Clamp((int) index, 0, component.Laws.Count - 1);

        if (index < 0)
            return;

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"The law '{GetLaw(uid, index.Value)}' has been removed from {ToPrettyString(uid).Name}!");

        component.Laws.RemoveAt((int) index);

        PinLaws(uid, component);
    }
}

public sealed class LawChangedEvent : EntityEventArgs
{
}

[UsedImplicitly]
[DataDefinition]
public sealed class ShowLaws : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var uiSystem = entityManager.System<UserInterfaceSystem>();

        if (!uiSystem.TryGetUi(uid, LawsUiKey.Key, out var ui))
            return;
        if (entityManager.TryGetComponent<ActorComponent>(uid, out var actorComponent))
            uiSystem.TryOpen(uid, LawsUiKey.Key, actorComponent.PlayerSession);
    }
}
