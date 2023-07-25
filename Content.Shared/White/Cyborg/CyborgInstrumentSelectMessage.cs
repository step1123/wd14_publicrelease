using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg
{
    [NetSerializable, Serializable]
    public enum CyborgInstrumentSelectUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class CyborgInstrumentSelectListState : BoundUserInterfaceState
    {
        public List<EntityUid> Instruments;


        public CyborgInstrumentSelectListState(List<EntityUid> instruments)
        {
            Instruments = instruments;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CyborgInstrumentSelectedMessage : BoundUserInterfaceMessage
    {
        public EntityUid SelectedInstrumentUid;

        public CyborgInstrumentSelectedMessage(EntityUid selectedInstrumentUid)
        {
            SelectedInstrumentUid = selectedInstrumentUid;
        }
    }
}
