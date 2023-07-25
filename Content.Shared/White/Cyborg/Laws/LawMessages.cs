using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Laws
{
    [NetSerializable, Serializable]
    public enum LawsUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class LawsUpdateState : BoundUserInterfaceState
    {
        public EntityUid Uid;
        public List<string> Laws;

        public LawsUpdateState(List<string> laws, EntityUid uid)
        {
            Laws = laws;
            Uid = uid;
        }
    }

    [Serializable, NetSerializable]
    public sealed class LawsUpdateMessage : BoundUserInterfaceMessage
    {
        public EntityUid Uid;
        public List<string> Laws;

        public LawsUpdateMessage(List<string> laws, EntityUid uid)
        {
            Laws = laws;
            Uid = uid;
        }
    }

    [Serializable, NetSerializable]
    public sealed class RemoveLawMessage : BoundUserInterfaceMessage
    {
        public EntityUid Uid;
        public int Index;

        public RemoveLawMessage(EntityUid uid, int index)
        {
            Uid = uid;
            Index = index;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReIndexLawMessage : BoundUserInterfaceMessage
    {
        public EntityUid Uid;
        public int Index;
        public int NewIndex;

        public ReIndexLawMessage(EntityUid uid, int index, int newIndex)
        {
            Uid = uid;
            Index = index;
            NewIndex = newIndex;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AddLawMessage : BoundUserInterfaceMessage
    {
        public EntityUid Uid;
        public string Law;
        public int? Index;

        public AddLawMessage(EntityUid uid, string law, int? index = null)
        {
            Uid = uid;
            Law = law;
            Index = index;
        }
    }

    [Serializable, NetSerializable]
    public sealed class StateLawsMessage : BoundUserInterfaceMessage
    {
        public List<string> Laws;

        public StateLawsMessage(List<string> laws)
        {
            Laws = laws;
        }
    }

    [Serializable, NetSerializable]
    public sealed class LawsBoundInterfaceState : BoundUserInterfaceState
    {}
}
