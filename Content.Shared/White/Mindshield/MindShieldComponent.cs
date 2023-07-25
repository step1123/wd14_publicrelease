using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Mindshield;

[RegisterComponent, NetworkedComponent]
public sealed class MindShieldComponent : Component
{
    public static string LayerName = "MindShieldHud";
}

[Serializable, NetSerializable]
public sealed class MindShieldComponentState : ComponentState { }

[RegisterComponent]
public sealed class ShowMindShieldHudComponent : Component { }

