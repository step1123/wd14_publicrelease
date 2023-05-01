using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Robust.Shared.Audio;

namespace Content.Server.White.Other.EnergySword;

public sealed class EnergyDoubleSwordCraftSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DoubleSwordCraftComponent, InteractUsingEvent>(Combine);
    }

    private const string NeededEnt = "EnergySword";
    private const string EnergyDoubleSword = "EnergyDoubleSword";

    private void Combine(EntityUid uid, DoubleSwordCraftComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        var user = args.User;
        var usedEnt = _entityManager.GetComponent<MetaDataComponent>(args.Used).EntityPrototype!.ID;
        var usedTo = _entityManager.GetComponent<MetaDataComponent>(uid).EntityPrototype!.ID;

        if (usedTo is EnergyDoubleSword)
            return;

        if (usedEnt != NeededEnt || usedTo != NeededEnt)
            return;

        DeleteUsed(args.Used, uid);
        SpawnEnergyDoubleSword(user);
    }


    private void DeleteUsed(EntityUid itemA, EntityUid itemB)
    {
        _entityManager.DeleteEntity(itemA);
        _entityManager.DeleteEntity(itemB);
    }

    private void SpawnEnergyDoubleSword(EntityUid player)
    {
        var transform = CompOrNull<TransformComponent>(player)?.Coordinates;

        if (transform == null)
            return;

        var weaponEntity = _entityManager.SpawnEntity(EnergyDoubleSword, transform.Value);
        _handsSystem.PickupOrDrop(player, weaponEntity);
    }
}
