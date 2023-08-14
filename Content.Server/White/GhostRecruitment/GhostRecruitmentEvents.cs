using Robust.Server.Player;
using Robust.Shared.Serialization;

namespace Content.Server.White.GhostRecruitment;

[Serializable]
public sealed class GhostRecruitmentSuccessEvent : EntityEventArgs
{
    public string RecruitmentName;
    public IPlayerSession PlayerSession;

    public GhostRecruitmentSuccessEvent(string recruitmentName, IPlayerSession playerSession)
    {
        RecruitmentName = recruitmentName;
        PlayerSession = playerSession;
    }
}
