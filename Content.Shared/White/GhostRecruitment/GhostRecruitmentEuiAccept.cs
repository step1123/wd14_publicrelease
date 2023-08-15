using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.White.GhostRecruitment
{

    [Serializable, NetSerializable]
    public enum AcceptRecruitmentUiButton
    {
        Deny,
        Accept,
    }

    [Serializable, NetSerializable]
    public sealed class AcceptRecruitmentChoiceMessage : EuiMessageBase
    {
        public readonly AcceptRecruitmentUiButton Button;

        public AcceptRecruitmentChoiceMessage(AcceptRecruitmentUiButton button)
        {
            Button = button;
        }
    }

}
