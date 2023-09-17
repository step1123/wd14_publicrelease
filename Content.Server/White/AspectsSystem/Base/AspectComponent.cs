using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.White.AspectsSystem.Base
{
    [RegisterComponent]
    public sealed class AspectComponent : Component
    {
        [DataField("name")]
        public string? Name;

        [DataField("description")]
        public string? Description;

        [DataField("requires")]
        public string? Requires;

        [DataField("weight")]
        public float Weight = 1.0f;

        [DataField("forbidden")]
        public bool IsForbidden;

        [DataField("hidden")]
        public bool IsHidden;

        [DataField("startAudio")]
        public SoundSpecifier? StartAudio;

        [DataField("endAudio")]
        public SoundSpecifier? EndAudio;

        [DataField("startDelay")]
        public TimeSpan StartDelay = TimeSpan.Zero;

        [DataField("startTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan StartTime;
    }
}
