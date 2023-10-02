using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Economy;

[RegisterComponent]
public sealed class EftposComponent : Component
{
    [ViewVariables]
    public int? BankAccountId;

    [ViewVariables]
    public int Amount;

    [DataField("soundApply")]
    public SoundSpecifier SoundApply = new SoundPathSpecifier("/Audio/White/Machines/chime.ogg");

    [DataField("soundDeny")]
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/White/Machines/buzz-sigh.ogg");
}

[Serializable, NetSerializable]
public enum EftposKey
{
    Key
}
