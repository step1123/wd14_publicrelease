using Robust.Shared.Containers;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class MMIComponent : Component
{
    [ViewVariables] public bool IsActive;
    [ViewVariables] public ContainerSlot BrainContainer = default!;
    public const string BrainContainerName = "brain-slot";
}
