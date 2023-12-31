using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Tag;
using Robust.Shared.Serialization;

namespace Content.Shared.Drone;

public class SharedDroneSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DroneComponent, InteractionAttemptEvent>(OnInteractionAttempt);
    }

    private void OnInteractionAttempt(EntityUid uid, DroneComponent component, InteractionAttemptEvent args)
    {
        if (args.Target != null && !HasComp<UnremoveableComponent>(args.Target) && !_tagSystem.HasAnyTag(
                args.Target.Value, "DroneUsable", "Trash", "Structure", "RCDDeconstructWhitelist", "Wall", "Window"))
            args.Cancel();
    }

    [Serializable, NetSerializable]
    public enum DroneVisuals : byte
    {
        Status
    }

    [Serializable, NetSerializable]
    public enum DroneStatus : byte
    {
        Off,
        On
    }
}
