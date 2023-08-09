using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.White.ServerEvent;

namespace Content.Server.White.ERTRecruitment;

public sealed class ERTRecruitmentAcceptEui : BaseEui
{
    private readonly EntityUid _uid;
    private readonly ERTRecruitmentSystem _ert;

    public ERTRecruitmentAcceptEui(EntityUid uid, ERTRecruitmentSystem system)
    {
        _uid = uid;
        _ert = system;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not ERTRecruitmentAcceptMessage.AcceptRecruitmentChoiceMessage choice ||
            choice.Button == ERTRecruitmentAcceptMessage.AcceptRecruitmentUiButton.Deny)
        {
            Close();
            return;
        }

        _ert.Recruit(_uid);
        Close();
    }
}
