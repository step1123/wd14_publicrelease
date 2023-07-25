namespace Content.Client.White.Cult.Items.VoidTorch;

[RegisterComponent]
public sealed class VoidTorchVisualsComponent : Component
{
    [DataField("stateOn")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateOn = "icon-on";

    [DataField("stateOff")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateOff = "icon";
}
