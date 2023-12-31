using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Laws.Component;

[RegisterComponent]
[NetworkedComponent]
public sealed class LawsComponent : Robust.Shared.GameObjects.Component
{
    [DataField("canState")] public bool CanState = true;

    [DataField("laws")] public List<string> Laws = new();
    [DataField("defaultLaws")] public List<string> DefaultLaws = new();

    [DataField("stateCD")] public TimeSpan StateCD = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Antispam.
    /// </summary>
    public TimeSpan? StateTime = null;
}

[Serializable]
[NetSerializable]
public sealed class LawsComponentState : ComponentState
{
    public readonly List<string> Laws;

    public LawsComponentState(List<string> laws)
    {
        Laws = laws;
    }
}
