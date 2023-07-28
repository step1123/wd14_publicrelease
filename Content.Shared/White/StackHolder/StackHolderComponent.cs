namespace Content.Shared.White.StackHolder;

[RegisterComponent]
public sealed class StackHolderComponent : Component
{
    [DataField("currentStackSlot")] public string CurrentStackSlot = "stack_slot";
}
