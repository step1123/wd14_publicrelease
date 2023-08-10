using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.White.GhostRecruitment;

namespace Content.Server.White.GhostRecruitment;

public sealed class GhostRecruitmentEuiAccept : BaseEui
{
    private readonly EntityUid _uid;
    private readonly string _recruitmentName;
    private readonly GhostRecruitmentSystem _recruitment;

    public GhostRecruitmentEuiAccept(EntityUid uid,string recruitmentName, GhostRecruitmentSystem system)
    {
        _uid = uid;
        _recruitmentName = recruitmentName;
        _recruitment = system;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not AcceptRecruitmentChoiceMessage choice ||
            choice.Button == AcceptRecruitmentUiButton.Deny)
        {
            Close();
            return;
        }

        _recruitment.Recruit(_uid,_recruitmentName);
        Close();
    }
}
