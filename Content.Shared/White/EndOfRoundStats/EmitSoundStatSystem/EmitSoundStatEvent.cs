using Robust.Shared.Audio;

namespace Content.Shared.White.EndOfRoundStats.EmitSoundStatSystem;

public sealed class EmitSoundStatEvent : EntityEventArgs
{
    public EntityUid Emitter;
    public SoundSpecifier Sound;

    public EmitSoundStatEvent(EntityUid emitter, SoundSpecifier sound)
    {
        Emitter = emitter;
        Sound = sound;
    }
}
