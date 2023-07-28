namespace Content.Shared.White.StackHolder;

[RegisterComponent]
public sealed class StackHolderListComponent : Component
{
    [DataField("stackSlots")] public List<string> StackSlots = new();
}
