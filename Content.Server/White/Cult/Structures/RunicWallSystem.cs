using Content.Server.Hands.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction;
using Content.Shared.White.Cult;

namespace Content.Server.White.Cult.Structures;

public sealed class RunicWallSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entMan = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RunicWallComponent, AfterInteractUsingEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, RunicWallComponent component, AfterInteractUsingEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
            return;
        if (MetaData(args.Used).EntityPrototype?.ID != component.UsedItemID)
            return;
        if (args.Target == null)
            return;

        var pos = Transform(args.Target.Value).Coordinates;
        _entMan.DeleteEntity(args.Target.Value);
        var ent = _entMan.SpawnEntity(component.DropItemID, pos);
        _hands.PickupOrDrop(args.User, ent, false);
        _entMan.SpawnEntity(component.SpawnStructureID, pos.SnapToGrid(_entMan));
    }
}
