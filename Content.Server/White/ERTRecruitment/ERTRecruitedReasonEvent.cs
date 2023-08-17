namespace Content.Server.White.ERTRecruitment;

[Serializable]
public sealed class ERTRecruitedReasonEvent : EntityEventArgs
{
    public string Reason = "";

    public void SetReason(string reason)
    {
        Reason = reason;
    }

}
