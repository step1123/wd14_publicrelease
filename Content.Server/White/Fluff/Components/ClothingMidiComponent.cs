using Content.Shared.Actions.ActionTypes;

namespace Content.Server.White.Fluff.Components;

[RegisterComponent]
public sealed class ClothingMidiComponent : Component
{
    [DataField("midiAction", required: true, serverOnly: true)] // server only, as it uses a server-BUI event !type
    public InstantAction? MidiAction;
}
