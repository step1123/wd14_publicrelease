using Robust.Shared.Serialization;

namespace Content.Shared.White.GhostRecruitment;

[Serializable,NetSerializable]
public sealed class GhostRecruitmentSuccessEvent : EntityEventArgs
{
    public string RecruitmentName;

    public GhostRecruitmentSuccessEvent(string recruitmentName)
    {
        RecruitmentName = recruitmentName;
    }
}

[Serializable,NetSerializable]
public sealed class GhostsRecruitmentSuccessEvent
{
    public string RecruitmentName;

    public GhostsRecruitmentSuccessEvent(string recruitmentName)
    {
        RecruitmentName = recruitmentName;
    }
}

[Serializable,NetSerializable]
public sealed class GhostsRecruitmentFailEvent
{
    public string RecruitmentName;

    public GhostsRecruitmentFailEvent(string recruitmentName)
    {
        RecruitmentName = recruitmentName;
    }
}

[Serializable,NetSerializable]
public sealed class GhostRecruitmentAttemptEvent : CancelableEventArgs
{
    public string RecruitmentName;

    public GhostRecruitmentAttemptEvent(string recruitmentName)
    {
        RecruitmentName = recruitmentName;
    }
}


[Serializable,NetSerializable]
public abstract class CancelableEventArgs
{
    /// <summary>
    ///     Whether this even has been cancelled.
    /// </summary>
    public bool Cancelled { get; private set; }

    /// <summary>
    ///     Cancels the event.
    /// </summary>
    public void Cancel() => Cancelled = true;

    /// <summary>
    ///     Uncancels the event. Don't call this unless you know what you're doing.
    /// </summary>
    public void Uncancel() => Cancelled = false;
}
