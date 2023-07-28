using Robust.Shared.Prototypes;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class ModuleComponentGrantingComponent : Component
{
    [DataField("component", required: true)]
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; } = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsActive = false;
}
