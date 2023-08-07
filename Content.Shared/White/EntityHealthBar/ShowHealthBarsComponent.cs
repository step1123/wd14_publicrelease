using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.EntityHealthBar
{
    /// <summary>
    /// This component allows you to see health bars above damageable mobs.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed class ShowHealthBarsComponent : Component
    {
        /// <summary>
        /// If null, displays all health bars.
        /// If not null, displays health bars of only that damage container.
        /// </summary>

        [DataField("damageContainer", customTypeSerializer: typeof(PrototypeIdSerializer<DamageContainerPrototype>))]
        public string? DamageContainer;
    }
}
