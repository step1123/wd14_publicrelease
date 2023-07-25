using System.Diagnostics.CodeAnalysis;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.White.Cult.Components;

[RegisterComponent, NetworkedComponent]
public sealed class CultEmpowerComponent : Component
{
    [DataField("isRune")]
    public bool IsRune;

    public int MaxAllowedCultistActions = 4;
    public int MinRequiredCultistActions = 1;
}

[Serializable, NetSerializable]
public sealed class CultEmpowerSelectedBuiMessage : BoundUserInterfaceMessage
{
    public ActionType ActionType;

    public CultEmpowerSelectedBuiMessage(ActionType actionType)
    {
        ActionType = actionType;
    }
}

[Serializable, NetSerializable]
public enum CultEmpowerUiKey : byte
{
    Key
}
