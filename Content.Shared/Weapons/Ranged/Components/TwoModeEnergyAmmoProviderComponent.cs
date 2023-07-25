using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent]
public sealed class TwoModeEnergyAmmoProviderComponent : BatteryAmmoProviderComponent
{
    [ViewVariables(VVAccess.ReadOnly),
     DataField("projProto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ProjectilePrototype = default!;

    [ViewVariables(VVAccess.ReadOnly),
     DataField("hitscanProto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string HitscanPrototype = default!;

    [ViewVariables(VVAccess.ReadOnly), DataField("projFireCost")]
    public float ProjFireCost = 50;

    [ViewVariables(VVAccess.ReadOnly), DataField("hitscanFireCost")]
    public float HitscanFireCost = 100;

    [ViewVariables(VVAccess.ReadOnly), DataField("currentMode")]
    public EnergyModes CurrentMode { get; set; } = EnergyModes.Stun;

    [ViewVariables(VVAccess.ReadOnly), DataField("projSound")]
    public SoundSpecifier? ProjSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/taser2.ogg");

    [ViewVariables(VVAccess.ReadOnly), DataField("hitscanSound")]
    public SoundSpecifier? HitscanSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/laser_cannon.ogg");

    public SoundSpecifier? ToggleSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Misc/egun_toggle.ogg");

    [ViewVariables(VVAccess.ReadOnly)] public bool InStun = true;
}

public enum EnergyModes
{
    Stun,
    Laser
}
