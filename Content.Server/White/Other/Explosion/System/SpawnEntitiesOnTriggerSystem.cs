using System.Numerics;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Server.White.Other.Explosion.Components;
using Content.Shared.Spawners.Components;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.White.Other.Explosion.System;

public sealed class SpawnEntitiesOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GunSystem _guns = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnEntitiesOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(EntityUid uid, SpawnEntitiesOnTriggerComponent component, TriggerEvent args)
    {
        if (component.Prototype == null)
            return;

        var prototype = _prototypes.Index<EntityPrototype>(component.Prototype);

        var spawnCount = component.Count;

        if (component.MinCount != null)
            spawnCount = _random.Next(component.MinCount.Value, component.Count);

        for (var i = 0; i < spawnCount; i++)
        {
            var spawnedEntity = EntityManager.SpawnEntity(prototype.ID, EntityManager.GetComponent<TransformComponent>(uid).Coordinates
                + new EntityCoordinates(EntityManager.GetComponent<TransformComponent>(uid).ParentUid, _random.NextVector2(-1, 1) * component.Offset));

            var transform = EntityManager.GetComponent<TransformComponent>(spawnedEntity);

            _transform.AttachToGridOrMap(spawnedEntity);

            if (component.Velocity != null)
            {
                transform.LocalRotation = Angle.FromDegrees(_random.Next(0, 360));

                if (component.Shoot)
                {
                    _guns.ShootProjectile(spawnedEntity, transform.LocalRotation.ToWorldVec(), Vector2.Zero, uid, uid, component.Velocity.Value * _random.NextFloat(0.8f, 1.2f));
                }
                else
                {
                    _throw.TryThrow(spawnedEntity, transform.LocalRotation.ToWorldVec(), 1.0f, uid, 5.0f);
                }
            }

            if (component.DespawnTime != null)
            {
                var timedDespawnComponent = EntityManager.EnsureComponent<TimedDespawnComponent>(spawnedEntity);
                timedDespawnComponent.Lifetime = component.DespawnTime.Value;
            }
        }
    }
}
