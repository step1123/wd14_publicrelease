namespace Content.Shared.White.Cyborg.Events;

public abstract class BatteryEvent : EntityEventArgs
{
    public EntityUid Battery;
    public EntityUid Entity;

    public BatteryEvent(EntityUid entity, EntityUid battery)
    {
        Entity = entity;
        Battery = battery;
    }
}

public sealed class BatteryLowEvent : BatteryEvent
{
    public BatteryLowEvent(EntityUid entity, EntityUid battery) : base(entity, battery)
    {
    }
}

public sealed class BatteryInsertedEvent : BatteryEvent
{
    public BatteryInsertedEvent(EntityUid entity, EntityUid battery) : base(entity, battery)
    {
    }
}
