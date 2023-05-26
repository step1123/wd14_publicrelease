using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Component = Robust.Shared.GameObjects.Component;

namespace Content.Shared.White.MeatyOre;

[Serializable, NetSerializable]
public sealed class MeatyOreShopRequestEvent : EntityEventArgs {}

[NetworkedComponent, RegisterComponent]
public sealed class IgnorBUIInteractionRangeComponent : Component
{

}
