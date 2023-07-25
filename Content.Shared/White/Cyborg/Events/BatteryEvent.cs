namespace Content.Shared.White.Cyborg.Events;

public class BatteryEvent : EntityEventArgs
{
    public EntityUid Battery;
    public EntityUid Entity;

    public BatteryEvent(EntityUid entity, EntityUid battery)
    {
        Entity = entity;
        Battery = battery;
    }
}
