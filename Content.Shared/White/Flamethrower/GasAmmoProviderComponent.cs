using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.Flamethrower;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class GasAmmoProviderComponent : AmmoProviderComponent
{
    [DataField("gasUsage", required: true)]
    [AutoNetworkedField]
    public float GasUsage;

    [ViewVariables]
    [AutoNetworkedField]
    public int Shots;

    [ViewVariables]
    [AutoNetworkedField]
    public int Capacity;

    [ViewVariables]
    [AutoNetworkedField]
    public float Pressure;

    [ViewVariables(VVAccess.ReadWrite), DataField("proto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    [AutoNetworkedField]
    public string Prototype = default!;
}
