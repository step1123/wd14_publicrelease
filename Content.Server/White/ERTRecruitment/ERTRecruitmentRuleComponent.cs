using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server.White.ERTRecruitment;

[RegisterComponent, Access(typeof(ERTRecruitmentRule))]
public sealed class ERTRecruitmentRuleComponent : Component
{
    public static string EventName = "ERTRecruitment";
    [ViewVariables]
    public MapId? MapId = null;

    [DataField("minPlayer")] public int MinPlayer = 4;

    public static SoundSpecifier ERTYes = new SoundPathSpecifier("/Audio/Announcements/ert_yes.ogg");
    public static SoundSpecifier ERTNo = new SoundPathSpecifier("/Audio/Announcements/ert_no.ogg");

    [ViewVariables]
    public bool IsBlocked = false;

    [ViewVariables]
    public EntityUid? Outpost;
    [ViewVariables]
    public EntityUid? Shuttle;
    [ViewVariables]
    public EntityUid? TargetStation;
}
