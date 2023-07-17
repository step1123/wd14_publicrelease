using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.White.Administration;
using Robust.Client.Console;
using Robust.Client.GameObjects;

namespace Content.Client.White.Administration;

public sealed class InvisibilitySystem : SharedInvisibilitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IClientConsoleHost _console = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InvisibilityComponent, ComponentInit>(OnInvisibilityInit);
        SubscribeLocalEvent<InvisibilityComponent, ComponentRemove>(OnInvisibilityRemove);

        SubscribeLocalEvent<InvisibilityComponent, ToggleInvisibilityActionEvent>(OnToggleGhosts);
        SubscribeNetworkEvent<InvisibilityToggleEvent>(OnInvisibilityToggle);
    }

    private void OnInvisibilityToggle(InvisibilityToggleEvent ev)
    {
        if (!EntityManager.TryGetComponent(ev.Uid, out SpriteComponent? sprite))
            return;

        var component = EntityManager.EnsureComponent<InvisibilityComponent>(ev.Uid);
        component.Invisible = ev.Invisible;
        component.DefaultAlpha ??= sprite.Color.A;

        var newAlpha = ev.Invisible ? component.DefaultAlpha.Value / 3f : component.DefaultAlpha.Value;
        sprite.Color = sprite.Color.WithAlpha(newAlpha);
    }

    private void OnInvisibilityInit(EntityUid uid, InvisibilityComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, component.ToggleInvisibilityAction, null);
    }

    private void OnInvisibilityRemove(EntityUid uid, InvisibilityComponent component, ComponentRemove args)
    {
        _actions.RemoveAction(uid, component.ToggleInvisibilityAction);
    }

    private void OnToggleGhosts(EntityUid uid, InvisibilityComponent component, ToggleInvisibilityActionEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupEntity("Невидимость переключена", args.Performer);
        _console.RemoteExecuteCommand(null, "invisibility");

        args.Handled = true;
    }
}
