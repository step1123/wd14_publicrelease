using Content.Shared.Interaction;
using Content.Shared.White.OutfitChangeState;

namespace Content.Server.White.OutfitChangeState;

public sealed class OutfitChangeStateSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<OutfitChangeStateComponent, InteractUsingEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, OutfitChangeStateComponent component, InteractUsingEvent args)
    {
        if (MetaData(args.Used).EntityPrototype?.ID != component.OutfitClicked)
            return;
        QueueDel(args.Used);
        UpdateAppearance(uid, OutfitChangeStatus.Active);
    }

    private void UpdateAppearance(EntityUid uid, OutfitChangeStatus status)
    {
        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearance.SetData(uid, OutfitChangeVisuals.Status, status, appearance);
    }
}
