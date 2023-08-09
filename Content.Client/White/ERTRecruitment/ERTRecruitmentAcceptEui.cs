using Content.Client.Eui;
using Content.Shared.Cloning;
using Content.Shared.White.ServerEvent;
using Robust.Client.Graphics;

namespace Content.Client.White.ERTRecruitment;

public sealed class ERTRecruitmentAcceptEui : BaseEui
{
    private readonly ERTRecruitmentAcceptWindow _window;

    public ERTRecruitmentAcceptEui()
    {
        _window = new ERTRecruitmentAcceptWindow();

        _window.DenyButton.OnPressed += _ =>
        {
            SendMessage(new ERTRecruitmentAcceptMessage.AcceptRecruitmentChoiceMessage(ERTRecruitmentAcceptMessage.AcceptRecruitmentUiButton.Deny));
            _window.Close();
        };

        _window.OnClose += () => SendMessage(new ERTRecruitmentAcceptMessage.AcceptRecruitmentChoiceMessage(ERTRecruitmentAcceptMessage.AcceptRecruitmentUiButton.Deny));

        _window.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new ERTRecruitmentAcceptMessage.AcceptRecruitmentChoiceMessage(ERTRecruitmentAcceptMessage.AcceptRecruitmentUiButton.Accept));
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
