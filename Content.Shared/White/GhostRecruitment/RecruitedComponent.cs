namespace Content.Shared.White.GhostRecruitment;


// this for spawned prototype
[RegisterComponent]
public sealed class RecruitedComponent : Component
{
    [DataField("recruitmentName")]
    public string RecruitmentName = "default";
}
