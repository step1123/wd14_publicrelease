using Content.Client.White.UserInterface.Controls;
using Content.Shared.White.Cyborg;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Cyborg.CyborgSelect;

[UsedImplicitly]
public sealed class CyborgSelectBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private RadialContainer? _radialContainer;


    public CyborgSelectBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _radialContainer = new RadialContainer();
        foreach (var prototype in _prototypeManager.EnumeratePrototypes<CyborgPrototype>())
        {
            var radialButton = _radialContainer.AddButton(prototype.CyborgName, prototype.Icon);
            radialButton.Controller.OnPressed += _ => Select(prototype.CyborgSpawnPrototype);
        }

        _radialContainer.OpenCentered();
        _radialContainer.Closed += Close;
        //var usrMngr = IoCManager.Resolve<IUserInterfaceManager>();
        //_radialContainer.Open(usrMngr.MousePositionScaled.Position);
    }

    public void Select(string prototype)
    {
        SendMessage(new CyborgSelectedMessage(prototype));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _radialContainer?.Dispose();
    }
}
