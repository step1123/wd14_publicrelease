using Content.Shared.Examine;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Administration;

public abstract class SharedInvisibilitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InvisibilityComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, InvisibilityComponent component, ExaminedEvent args)
    {
        if (component.Invisible)
            args.PushMarkup("[color=lightsteelblue]Оно доступно лишь взору богов.[/color]");
    }
}

[Serializable, NetSerializable]
public sealed class InvisibilityToggleEvent : EntityEventArgs
{
    public EntityUid Uid { get; }
    public bool Invisible { get; }

    public InvisibilityToggleEvent(EntityUid uid, bool invisible)
    {
        Uid = uid;
        Invisible = invisible;
    }
}
