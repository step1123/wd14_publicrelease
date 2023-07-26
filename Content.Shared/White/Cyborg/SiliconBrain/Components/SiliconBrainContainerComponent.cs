using Robust.Shared.Containers;

namespace Content.Shared.White.Cyborg.SiliconBrain.Components;

[RegisterComponent]
public sealed class SiliconBrainContainerComponent : Component
{
    public const string BrainSlotId = "brain-slot";

    [ViewVariables] public ContainerSlot BrainSlot = default!;

    [ViewVariables] public EntityUid? BrainUid;
}
