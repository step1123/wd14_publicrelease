using System.Linq;
using Content.Client.White.UserInterface.Controls;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Structures;
using Content.Shared.White.Cult.UI;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Cult.UI.CultistFactory;

public sealed class CultistFactoryBUI : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private RadialContainer? _radialContainer;

    private bool _updated = false;

    public CultistFactoryBUI(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }
    private void ResetUI()
    {
        _radialContainer?.Close();
        _radialContainer = null;
        _updated = false;
    }

    protected override void Open()
    {
        base.Open();

        if (_radialContainer != null)
            ResetUI();

        _radialContainer = new RadialContainer();

        if (State != null)
            UpdateState(State);
    }

    private void PopulateRadial(IReadOnlyCollection<string> ids)
    {
        foreach (var id in ids)
        {
            if (!_prototypeManager.TryIndex<CultistFactoryProductionPrototype>(id, out var prototype))
                return;

            if (_radialContainer == null)
                continue;

            var button = _radialContainer.AddButton(prototype.Name, prototype.Icon);
            button.Controller.OnPressed += _ =>
            {
                Select(id);
            };
        }
    }

    private void Select(string id)
    {
        SendMessage(new CultistFactoryItemSelectedMessage(id));
        ResetUI();
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ResetUI();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_updated)
            return;

        if (state is CultistFactoryBUIState newState)
        {
            PopulateRadial(newState.Ids);
        }

        if (_radialContainer == null)
            return;

        _radialContainer?.OpenAttachedLocalPlayer();
        _updated = true;
    }
}
