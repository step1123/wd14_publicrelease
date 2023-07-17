using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Utility;

namespace Content.Shared.White.Administration;

[RegisterComponent]
[Access(typeof(SharedInvisibilitySystem))]
public sealed class InvisibilityComponent : Component
{
    public bool Invisible;

    public readonly InstantAction ToggleInvisibilityAction = new()
    {
        Icon = new SpriteSpecifier.Texture(new("White/Icons/transparent-ghost.png")),
        DisplayName = "Переключить невидимость",
        Description = "Переключить невидимость вашего призрака.",
        ClientExclusive = true,
        CheckCanInteract = false,
        Event = new ToggleInvisibilityActionEvent()
    };
}

public sealed class ToggleInvisibilityActionEvent : InstantActionEvent
{
}
