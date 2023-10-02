using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Economy;

[RegisterComponent]
public sealed class ATMComponent : Component
{
    [DataField("idCardSlot")]
    public ItemSlot CardSlot = default!;

    [DataField("currencyType")]
    public string CurrencyType = "SpaceCash";

    public string SlotId = "card-slot";

    [ValidatePrototypeId<StackPrototype>]
    public string CreditStackPrototype = "Credit";

    [DataField("soundInsertCurrency")]
    public SoundSpecifier SoundInsertCurrency = new SoundPathSpecifier("/Audio/White/Machines/polaroid2.ogg");

    [DataField("soundWithdrawCurrency")]
    public SoundSpecifier SoundWithdrawCurrency = new SoundPathSpecifier("/Audio/White/Machines/polaroid1.ogg");

    [DataField("soundApply")]
    public SoundSpecifier SoundApply = new SoundPathSpecifier("/Audio/White/Machines/chime.ogg");

    [DataField("soundDeny")]
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/White/Machines/buzz-sigh.ogg");
}


[Serializable, NetSerializable]
public enum ATMUiKey
{
    Key
}
