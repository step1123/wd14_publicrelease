using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Component = Robust.Shared.GameObjects.Component;

namespace Content.Shared.White.MeatyOre;

    [Serializable, NetSerializable]
    public sealed class MeatyOreShopRequestEvent : EntityEventArgs {}

[Serializable, NetSerializable]
public sealed class MeatyTraitorRequestActionEvent
{
    public override bool Equals(object? obj)
    {
        return true;
    }
}

[NetworkedComponent, RegisterComponent]
public sealed class IgnorBUIInteractionRangeComponent : Component
{

}
