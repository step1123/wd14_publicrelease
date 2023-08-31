using Content.Shared.Access.Systems;
using Content.Shared.PDA;
using Robust.Shared.GameStates;

namespace Content.Shared.Access.Components
{
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    [Access(typeof(SharedIdCardSystem), typeof(SharedPdaSystem), typeof(SharedAgentIdCardSystem), Other = AccessPermissions.ReadWrite)]
    public sealed partial class IdCardComponent : Component
    {
        [DataField("fullName")]
        [AutoNetworkedField]
        // FIXME Friends
        public string? FullName;

        //WD-EDIT
        /// <summary>
        /// Sets custom icon for HUDs.
        /// </summary>
        [DataField("customIcon")]
        [AutoNetworkedField]
        public string? CustomIcon;
        //WD-EDIT

        [DataField("jobTitle")]
        [AutoNetworkedField]
        [Access(typeof(SharedIdCardSystem), typeof(SharedPdaSystem), typeof(SharedAgentIdCardSystem), Other = AccessPermissions.ReadWrite)]
        public string? JobTitle;
    }
}
