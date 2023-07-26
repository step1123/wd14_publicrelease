using Robust.Shared.Containers;

namespace Content.Shared.White.Cyborg.SiliconBrain.Components;

[RegisterComponent]
public sealed class MMIComponent : Component
{
    public const string BrainContainerName = "brain-slot";
    [ViewVariables] public ContainerSlot BrainContainer = default!;
    [ViewVariables] public bool IsActive;
}
