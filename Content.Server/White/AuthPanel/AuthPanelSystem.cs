using Content.Server.Access.Systems;
using Content.Server.Popups;
using Content.Shared.Access.Systems;
using Content.Shared.White.AuthPanel;
using Robust.Server.GameObjects;

namespace Content.Server.White.AuthPanel;

public sealed class AuthPanelSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public Dictionary<AuthPanelAction, HashSet<EntityUid>> Counter = new();
    public static int MaxCount = 2;
    public override void Initialize()
    {
        SubscribeLocalEvent<AuthPanelComponent,AuthPanelButtonPressedMessage>(OnButtonPressed);
        SubscribeLocalEvent<AuthPanelComponent,AuthPanelPerformActionEvent>(OnPerformAction);
    }

    private void OnPerformAction(EntityUid uid, AuthPanelComponent component, AuthPanelPerformActionEvent args)
    {
        Logger.Debug("Performed action " + args.Action);
    }

    private void OnButtonPressed(EntityUid uid, AuthPanelComponent component, AuthPanelButtonPressedMessage args)
    {
        if(args.Session.AttachedEntity == null)
            return;

        if (!_access.FindAccessTags(args.Session.AttachedEntity.Value).Contains("Command"))
        {
            _popup.PopupEntity("Нет доступа",
                args.Session.AttachedEntity.Value,args.Session.AttachedEntity.Value);
            return;
        }

        if (!Counter.TryGetValue(args.Button, out var hashSet))
        {
            hashSet = new HashSet<EntityUid>();
            Counter.Add(args.Button,hashSet);
        }

        if(hashSet.Count == MaxCount)
            return;

        if (!hashSet.Add(args.Session.AttachedEntity.Value))
        {
            _popup.PopupEntity("Вы уже нажали на эту кнопку",
                args.Session.AttachedEntity.Value,args.Session.AttachedEntity.Value);
            return;
        }

        UpdateUserInterface();

        if (hashSet.Count == MaxCount)
        {
            var ev = new AuthPanelPerformActionEvent(args.Button);
            RaiseLocalEvent(uid,ev);
        }

    }

    public void UpdateUserInterface()
    {
        var actions = new HashSet<AuthPanelConfirmationAction>();

        foreach (var (action,hashSet) in Counter)
        {
            actions.Add(new AuthPanelConfirmationAction(action,hashSet.Count,MaxCount));
        }

        var query = EntityQueryEnumerator<AuthPanelComponent>();
        while (query.MoveNext(out var uid,out _))
        {
            if (!_ui.HasUi(uid, AuthPanelUiKey.Key))
                return;

            var state = new AuthPanelConfirmationActionState(actions);
            var ui = _ui.GetUi(uid, AuthPanelUiKey.Key);

            UserInterfaceSystem.SetUiState(ui, state);
            _appearance.SetData(uid,AuthPanelVisualLayers.Confirm,true);
        }
    }
}
