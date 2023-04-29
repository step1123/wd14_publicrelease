using Content.Client.Administration.Managers;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Client.White.Sponsors;
using Content.Shared.Administration;
using Content.Shared.Humanoid;
using Content.Shared.White.MeatyOre;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client.White.MeatyOre;

public sealed class MeatyOreUIController : UIController
{
    [Dependency] private readonly IClientAdminManager _clientAdminManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityNetworkManager _entityNetworkManager = default!;
    [Dependency] private readonly SponsorsManager _sponsorsManager = default!;

    private bool _buttonLoaded;

    private MenuButton? MeatyOreButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.MeatyOreButton;
    public void LoadButton()
    {
        MeatyOreButton!.OnPressed += MeatyOreButtonPressed;
        _buttonLoaded = true;
    }

    public void UnloadButton()
    {
        MeatyOreButton!.OnPressed -= MeatyOreButtonPressed;
        _buttonLoaded = false;
    }

    private void MeatyOreButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        _entityNetworkManager.SendSystemNetworkMessage(new MeatyOreShopRequestEvent());
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if(!_buttonLoaded) return;
        var shouldBeVisible = CheckButtonVisibility();
        MeatyOreButton!.Visible = shouldBeVisible;
    }


    private bool CheckButtonVisibility()
    {
        if(!_sponsorsManager.TryGetInfo(out var sponsor)) return false;
        if(sponsor?.Tier == null || sponsor?.MeatyOreCoin == 0) return false;

        var controlledEntity = _playerManager!.LocalPlayer!.ControlledEntity;
        if(controlledEntity == null) return false;

        if (!_entityManager.HasComponent<HumanoidAppearanceComponent>(controlledEntity)) return false;

        return true;
    }
}
