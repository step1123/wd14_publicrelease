using Content.Shared.Interaction;
using Content.Shared.White.Cult;

namespace Content.Server.White.Cult.Structures;

public sealed class RunicGirderSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RunicGirderComponent, AfterInteractUsingEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, RunicGirderComponent component, AfterInteractUsingEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
            return;
        if (MetaData(args.Used).EntityPrototype?.ID != component.UsedItemID)
            return;
        if (args.Target == null)
            return;

        var pos = Transform(args.Target.Value).Coordinates;
        _entMan.DeleteEntity(args.Target.Value);
        _entMan.SpawnEntity(component.DropItemID, pos);
    }
}
