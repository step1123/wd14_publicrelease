using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid.Prototypes
{
    [Prototype("bodyType")]
    public sealed class BodyTypePrototype : IPrototype
    {
        /// <summary>
        ///     Which sex can't use this body type.
        /// </summary>
        [DataField("sexRestrictions")]
        public List<string> SexRestrictions = new();

        /// <summary>
        ///     Sprites that this species will use on the given humanoid
        ///     visual layer. If a key entry is empty, it is assumed that the
        ///     visual layer will not be in use on this species, and will
        ///     be ignored.
        /// </summary>
        [DataField("sprites", required: true)]
        public Dictionary<HumanoidVisualLayers, string> Sprites = new();

        /// <summary>
        ///     User visible name of the body type.
        /// </summary>
        [DataField("name", required: true)]
        public string Name { get; } = default!;

        /// <summary>
        ///     Prototype ID of the body type.
        /// </summary>
        [IdDataField]
        public string ID { get; } = default!;
    }
}
