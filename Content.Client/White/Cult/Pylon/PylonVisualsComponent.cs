using Content.Client.Storage.Visualizers;

namespace Content.Client.White.Cult.Pylon;

[RegisterComponent]
public sealed class PylonVisualsComponent : Component
{
    [DataField("stateOn")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateOn = "pylon";

    [DataField("stateOff")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateOff = "pylon_off";
}
