using Robust.Shared.GameStates;

namespace Content.Shared.White.Crossbow;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PenetratedComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public EntityUid? ProjectileUid;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool IsPinned;
}
