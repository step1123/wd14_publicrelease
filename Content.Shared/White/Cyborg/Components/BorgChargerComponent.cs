using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Components;

public struct BorgChargerVisualInfo
{
    public BorgChargerComponent.BorgChargerVisuals MachineLayer;
    public BorgChargerComponent.BorgChargerVisuals TerminalLayer;
}

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed class BorgChargerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("chargeRate")]
    public int ChargeRate = 5;

    [ViewVariables(VVAccess.ReadOnly)]
    public ContainerSlot BodyContainer = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("removeSound")]
    public SoundSpecifier RemoveSound = new SoundPathSpecifier("/Audio/Items/pistol_magout.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("insertSound")]
    public SoundSpecifier InsertSound = new SoundPathSpecifier("/Audio/Items/pistol_magin.ogg");

    [Serializable, NetSerializable]
    public enum BorgChargerVisuals : byte
    {
        Base,
        Light,
        // based
        Closed,
        Opened,
        // other
        ClosePowered,
        OpenPowered,
        CloseUnpowered,
        OpenUnpowered,
        Occupied,
        OccupiedCharged
    }
}

/// <summary>
/// Contains network state for BorgChargerComponent.
/// </summary>
[Serializable, NetSerializable]
public sealed class BorgChargerComponentState : ComponentState
{
    public BorgChargerComponentState(BorgChargerComponent component)
    {

    }
}
