using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg;

[Serializable, NetSerializable]
public sealed class CyborgActionStatus
{
    public CyborgActionStatus(Enum action, string address, EntityUid? actioner)
    {
        Action = action;
        Address = address;
        Actioner = actioner;
    }

    public Enum Action;
    public string Address;
    public EntityUid? Actioner;

}

public static class CyborgActionConstants
{
    public const string NET_ACTION = "action";
    public const string NET_ADDRESS = "address";
    public const string NET_ACTIONER = "actioner";
}

