namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
public sealed class CyborgModuleComponent : Component
{
    [DataField("name")] public string Name { get; private set; } = "Module";
    [ViewVariables] public EntityUid? Parent = null;
}
