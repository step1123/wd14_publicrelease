using Robust.Shared.GameStates;

namespace Content.Shared.White.ClothingGrant.Components
{
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    public sealed partial class ClothingGrantTagComponent : Component
    {
        [DataField("tag", required: true), ViewVariables(VVAccess.ReadWrite)]
        public string Tag = "";

        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public bool IsActive = false;
    }
}
