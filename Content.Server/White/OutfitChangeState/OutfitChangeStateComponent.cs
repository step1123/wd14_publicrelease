namespace Content.Server.White.OutfitChangeState;

[RegisterComponent]
public sealed class OutfitChangeStateComponent : Component
{
    [DataField("outfitClicked", required: true)]
    public string OutfitClicked = default!;
}
