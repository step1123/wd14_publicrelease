namespace Content.Server.White.Other.Explosion.Components;

[RegisterComponent]
public sealed class SpawnEntitiesOnTriggerComponent : Component
{
    /// <summary>
    ///     The prototype of the entity to spawn.
    /// </summary>
    [DataField("prototype")]
    public string? Prototype;

    /// <summary>
    ///     The amount of entities to spawn.
    /// </summary>
    [DataField("count")]
    public int Count = 1;

    /// <summary>
    ///     The minimum amount of entities to spawn.
    ///     If this is null, will always spawn full amount.
    /// </summary>
    [DataField("minCount")]
    public int? MinCount;

    /// <summary>
    ///     The velocity to either shoot or throw the spawned entities with.
    ///     If this is null, entities won't be thrown or shot.
    /// </summary>
    [DataField("velocity")]
    public float? Velocity;

    /// <summary>
    ///     The time in seconds before the spawned entities despawn.
    ///     If this is null, entities won't despawn.
    /// </summary>
    [DataField("despawnTime")]
    public float? DespawnTime;

    /// <summary>
    ///     Whether or to shoot the spawned entities.
    ///     If this is false, entities with velocity will be thrown.
    /// </summary>
    [DataField("shoot")]
    public bool Shoot;

    /// <summary>
    ///     The offset multiplier for the spawned entities in max tiles.
    ///     If this is 0, entities will spawn on top of the trigger.
    /// </summary>
    [DataField("offset")]
    public float Offset;
}
