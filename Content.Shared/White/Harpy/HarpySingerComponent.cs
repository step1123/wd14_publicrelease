using Robust.Shared.GameStates;
using Content.Shared.Actions.ActionTypes; //WD EDIT

namespace Content.Shared.White.Harpy
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class HarpySingerComponent : Component
    {
        //[DataField("midiActionId", serverOnly: true,
        //    customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        //public string? MidiActionId = "ActionHarpyPlayMidi";

        [DataField("midiAction", required: true, serverOnly: true)] // server only, as it uses a server-BUI event !type
        public InstantAction? MidiAction; //WD EDIT
    }
}
