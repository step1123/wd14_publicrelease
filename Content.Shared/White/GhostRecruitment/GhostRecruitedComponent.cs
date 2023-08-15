namespace Content.Shared.White.GhostRecruitment;

//this for ghosts
[RegisterComponent]
public sealed class GhostRecruitedComponent : Component
{
    [DataField("recruitmentName")]
    public string RecruitmentName = "default";
}
