using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.White.SaltedYayca;

public sealed class DiscordAuthResponse : NetMessage
{
    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableUnordered;
    public override MsgGroups MsgGroup => MsgGroups.String;

    public string Uri = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Uri = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Uri);
    }
}
