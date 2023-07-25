namespace Content.Shared.White.Cyborg.Events;

public sealed class BatteryInsertedEvent: BatteryEvent
{
    public BatteryInsertedEvent(EntityUid entity, EntityUid battery) : base(entity, battery)
    {
    }
}
