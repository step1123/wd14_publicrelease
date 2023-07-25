namespace Content.Shared.White.Cyborg.Events;

public sealed class BatteryLowEvent : EntityEventArgs
{
    public EntityUid Entity;
    public EntityUid? Battery;
    public BatteryLowEvent(EntityUid entity, EntityUid? battery)
    {
        Entity = entity;
        Battery = battery;
    }
}
