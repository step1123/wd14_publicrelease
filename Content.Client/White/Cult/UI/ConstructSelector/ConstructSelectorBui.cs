using System.Linq;
using Content.Client.White.UserInterface.Controls;
using Content.Shared.White.Cult.Runes.Components;
using Content.Shared.White.Cult.UI;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Cult.UI.ConstructSelector;

public sealed class ConstructSelectorBui : BoundUserInterface
{

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    private SpriteSystem _spriteSystem = default!;

    private bool _selected;

    public ConstructSelectorBui(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _spriteSystem = _entityManager.EntitySysManager.GetEntitySystem<SpriteSystem>();
        var shellComponent = _entityManager.GetComponent<ConstructShellComponent>(Owner.Owner);

        var shellSelector = new RadialContainer();

        shellSelector.Closed += () =>
        {
            if(_selected) return;

            SendMessage(new ConstructFormSelectedEvent(shellComponent.ConstructForms.First()));
        };

        foreach (var form in shellComponent.ConstructForms)
        {
            var formPrototype = _prototypeManager.Index<EntityPrototype>(form);
            var button = shellSelector.AddButton(formPrototype.Name, _spriteSystem.GetPrototypeIcon(formPrototype).Default);

            button.Controller.OnPressed += _ =>
            {
                _selected = true;
                SendMessage(new ConstructFormSelectedEvent(form));
                shellSelector.Close();
            };
        }

        shellSelector.OpenCentered();
    }
}
