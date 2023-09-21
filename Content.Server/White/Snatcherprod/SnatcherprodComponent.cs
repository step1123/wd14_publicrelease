using Robust.Shared.Audio;

namespace Content.Server.White.Snatcherprod
{
    [RegisterComponent, Access(typeof(SnatcherprodSystem))]
    public sealed class SnatcherprodComponent : Component
    {
        public bool Activated = false;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("energyPerUse")]
        public float EnergyPerUse { get; set; } = 36;

        [DataField("stunSound")]
        public SoundSpecifier StunSound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/egloves.ogg");

        [DataField("sparksSound")]
        public SoundSpecifier SparksSound { get; set; } = new SoundCollectionSpecifier("sparks");

        [DataField("turnOnFailSound")]
        public SoundSpecifier TurnOnFailSound { get; set; } = new SoundPathSpecifier("/Audio/Machines/button.ogg");
    }
}
