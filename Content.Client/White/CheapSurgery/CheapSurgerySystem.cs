using Content.Client.White.UserInterface.Controls;
using Content.Shared.White.CheapSurgery;
using Robust.Client.GameObjects;

namespace Content.Client.White.CheapSurgery;

public sealed class CheapSurgerySystem : SharedCheapSurgerySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<OnSurgeryStarted>(OnStarted);
    }

    private void OnStarted(OnSurgeryStarted ev)
    {
        OpenRadialMenu(ev.OrganItems);
    }

    public void OpenRadialMenu(List<OrganItem> items)
    {
        if(items.Count == 0)
            return;

        var radialContainer = new RadialContainer();
        foreach (var organ in items)
        {
            var radialButton = radialContainer.AddButton(organ.Name, _sprite.Frame0(organ.Icon));
            radialButton.Controller.OnPressed += _ =>
            {
                radialContainer.Close();
                if (organ.Children.Count > 0)
                    OpenRadialMenu(organ.Children);
                else
                    SelectOrgan(organ.Uid);
            };
        }

        radialContainer.OpenCentered();
    }
    public void SelectOrgan(EntityUid uid)
    {
        var ev = new OnOrganSelected(uid);
        RaiseNetworkEvent(ev);
    }
}
