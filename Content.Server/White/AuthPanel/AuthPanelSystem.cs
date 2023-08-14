using Content.Server.Access.Systems;
using Content.Server.Popups;
using Content.Server.White.ERTRecruitment;
using Content.Server.White.ServerEvent;
using Content.Shared.Access.Systems;
using Content.Shared.GameTicking;
using Content.Shared.White.AuthPanel;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.White.AuthPanel;

public sealed class AuthPanelSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly ServerEventSystem _event = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ERTRecruitmentSystem _ert = default!;

    public Dictionary<AuthPanelAction, HashSet<EntityUid>> Counter = new();
    public Dictionary<AuthPanelAction, HashSet<int>> CardIndexes = new();

    public static int MaxCount = 2;

    private TimeSpan? _delay;

    public override void Initialize()
    {
        SubscribeLocalEvent<AuthPanelComponent,AuthPanelButtonPressedMessage>(OnButtonPressed);
        SubscribeLocalEvent<AuthPanelComponent,AuthPanelPerformActionEvent>(OnPerformAction);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        Counter.Clear();
        CardIndexes.Clear();

        _delay = null;
    }

    private void OnPerformAction(EntityUid uid, AuthPanelComponent component, AuthPanelPerformActionEvent args)
    {
        if (args.Action is AuthPanelAction.ERTRecruit)
        {
            if (_random.Next(0, 10) < 2)
            {
                _event.TryStartEvent(ERTRecruitmentSystem.EventName);
            }
            else
            {
                _ert.DeclineERT();
            }
        }
    }

    private void OnButtonPressed(EntityUid uid, AuthPanelComponent component, AuthPanelButtonPressedMessage args)
    {
        if(args.Session.AttachedEntity == null)
            return;

        var access = _access.FindAccessTags(args.Session.AttachedEntity.Value);

        if (!access.Contains("Command"))
        {
            _popup.PopupEntity("Нет доступа",
                args.Session.AttachedEntity.Value,args.Session.AttachedEntity.Value);
            return;
        }

        if (_delay != null)
        {
            _popup.PopupEntity("Пожалуйста, подождите прежде чем активировать кнопку",
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

        if (!CardIndexes.TryGetValue(args.Button, out var cardSet))
        {
            cardSet = new HashSet<int>();
            CardIndexes.Add(args.Button,cardSet);
        }

        if (cardSet.Contains(access.Count))
        {
            _popup.PopupEntity("Похоже, вы уже использовали данную ID карту",
                args.Session.AttachedEntity.Value,args.Session.AttachedEntity.Value);
            return;
        }

        if (!hashSet.Add(args.Session.AttachedEntity.Value))
        {
            _popup.PopupEntity("Вы уже нажали на эту кнопку",
                args.Session.AttachedEntity.Value,args.Session.AttachedEntity.Value);
            return;
        }

        cardSet.Add(access.Count);
        _delay = _timing.CurTime + TimeSpan.FromSeconds(5);

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

    public override void Update(float frameTime)
    {
        if (_delay == null)
            return;

        if (_timing.CurTime >= _delay)
        {
            _delay = null;
        }
    }
}
