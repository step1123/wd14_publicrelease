using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Laws;

[NetSerializable]
[Serializable]
public enum LawsUiKey : byte
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class LawsUpdateState : BoundUserInterfaceState
{
    public List<string> Laws;
    public EntityUid Uid;

    public LawsUpdateState(List<string> laws, EntityUid uid)
    {
        Laws = laws;
        Uid = uid;
    }
}

[Serializable]
[NetSerializable]
public sealed class LawsUpdateMessage : BoundUserInterfaceMessage
{
    public List<string> Laws;
    public EntityUid Uid;

    public LawsUpdateMessage(List<string> laws, EntityUid uid)
    {
        Laws = laws;
        Uid = uid;
    }
}

[Serializable]
[NetSerializable]
public sealed class RemoveLawMessage : BoundUserInterfaceMessage
{
    public int Index;
    public EntityUid Uid;

    public RemoveLawMessage(EntityUid uid, int index)
    {
        Uid = uid;
        Index = index;
    }
}

[Serializable]
[NetSerializable]
public sealed class ReIndexLawMessage : BoundUserInterfaceMessage
{
    public int Index;
    public int NewIndex;
    public EntityUid Uid;

    public ReIndexLawMessage(EntityUid uid, int index, int newIndex)
    {
        Uid = uid;
        Index = index;
        NewIndex = newIndex;
    }
}

[Serializable]
[NetSerializable]
public sealed class AddLawMessage : BoundUserInterfaceMessage
{
    public int? Index;
    public string Law;
    public EntityUid Uid;

    public AddLawMessage(EntityUid uid, string law, int? index = null)
    {
        Uid = uid;
        Law = law;
        Index = index;
    }
}

[Serializable]
[NetSerializable]
public sealed class StateLawsMessage : BoundUserInterfaceMessage
{
    public List<string> Laws;

    public StateLawsMessage(List<string> laws)
    {
        Laws = laws;
    }
}

[Serializable]
[NetSerializable]
public sealed class LawsBoundInterfaceState : BoundUserInterfaceState
{
}
