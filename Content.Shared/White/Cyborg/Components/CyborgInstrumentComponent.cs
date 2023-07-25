using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class CyborgInstrumentComponent : Component
{

    [ViewVariables]
    public EntityUid CyborgUid = EntityUid.Invalid;

    [ViewVariables]
    public EntityUid? BatteryUid = null;

    [DataField("nextUpdate", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime;
}
