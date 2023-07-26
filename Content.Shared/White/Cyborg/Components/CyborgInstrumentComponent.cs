using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class CyborgInstrumentComponent : Component
{
    [ViewVariables] public EntityUid? BatteryUid = null;

    [ViewVariables] public EntityUid CyborgUid = EntityUid.Invalid;

    [DataField("nextUpdate", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime;
}
