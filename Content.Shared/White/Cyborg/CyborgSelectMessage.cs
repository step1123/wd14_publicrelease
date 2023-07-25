using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg
{
    [NetSerializable, Serializable]
    public enum CyborgSelectUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class CyborgSelectListState : BoundUserInterfaceState
    {
        public HashSet<string> Prototypes;


        public CyborgSelectListState(HashSet<string> prototypes)
        {
            Prototypes = prototypes;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CyborgSelectedMessage : BoundUserInterfaceMessage
    {
        public string SelectedPolyMorph;

        public CyborgSelectedMessage(string selectedPolyMorph)
        {
            SelectedPolyMorph = selectedPolyMorph;
        }
    }
}
