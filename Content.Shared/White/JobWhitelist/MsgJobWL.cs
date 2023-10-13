using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.White.JobWhitelist;

public sealed class MsgJobWL : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public List<string> AllowedJobs = new();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var count = buffer.ReadVariableInt32();
        AllowedJobs.EnsureCapacity(count);

        for (var i = 0; i < count; i++)
        {
            AllowedJobs.Add(buffer.ReadString());
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(AllowedJobs.Count);

        foreach (var job in AllowedJobs)
        {
            buffer.Write(job);
        }
    }
}
