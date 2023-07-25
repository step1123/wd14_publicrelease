namespace Content.Client.White.Cult.Items.VeilShifter;

[RegisterComponent]
public sealed class VeilVisualsComponent : Component
{
    [DataField("stateOn")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateOn = "icon-on";

    [DataField("stateOff")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateOff = "icon";
}
