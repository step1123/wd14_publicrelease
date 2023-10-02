using Content.Client.UserInterface.Fragments;
using Content.Shared.White.Economy;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.White.Economy.Ui;

public sealed class BankUi : UIFragment
{
    private BankUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new BankUiFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not BankCartridgeUiState bankState)
            return;

        _fragment?.UpdateState(bankState);
    }
}
