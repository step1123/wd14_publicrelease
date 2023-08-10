using Content.Client.Eui;
using Content.Shared.Cloning;
using Content.Shared.White.GhostRecruitment;
using Robust.Client.Graphics;

namespace Content.Client.White.GhostRecruitment;

public sealed class GhostRecruitmentEuiAccept : BaseEui
{
    private readonly GhostRecruitmentEuiAcceptWindow _window;

    public GhostRecruitmentEuiAccept()
    {
        _window = new GhostRecruitmentEuiAcceptWindow();

        _window.DenyButton.OnPressed += _ =>
        {
            SendMessage(new AcceptRecruitmentChoiceMessage(AcceptRecruitmentUiButton.Deny));
            _window.Close();
        };

        _window.OnClose += () => SendMessage(new AcceptRecruitmentChoiceMessage(AcceptRecruitmentUiButton.Deny));

        _window.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new AcceptRecruitmentChoiceMessage(AcceptRecruitmentUiButton.Accept));
            _window.Close();
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }
}
