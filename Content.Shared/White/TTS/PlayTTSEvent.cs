using Robust.Shared.Serialization;

namespace Content.Shared.White.TTS;

[Serializable, NetSerializable]
// ReSharper disable once InconsistentNaming
public sealed class PlayTTSEvent : EntityEventArgs
{
    public EntityUid Uid { get; }
    public byte[] Data { get; }
    public bool BoostVolume { get; }

    public PlayTTSEvent(EntityUid uid, byte[] data, bool boostVolume)
    {
        Uid = uid;
        Data = data;
        BoostVolume = boostVolume;
    }
}
