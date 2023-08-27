using Content.Server.GameTicking.Presets;
using Content.Server.White.Reva.Systems;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.White.Reva.Components;

[RegisterComponent, Access(typeof(RevolutionaryRuleSystem))]
public sealed class RevolutionaryRuleComponent : Component
{
    [DataField("minPlayers")]
    public int MinPlayers = 20;

    [DataField("playersPerHeadRev")]
    public int PlayersPerHeadRev = 10;

    [DataField("maxHeadRevs")]
    public int MaxHeadRev = 5;

    [DataField("greetingSound", customTypeSerializer: typeof(SoundSpecifierTypeSerializer))]
    public SoundSpecifier? GreetSound = new SoundPathSpecifier("/Audio/White/Revolution/reva_start.ogg");

    [DataField("winType")]
    public WinType WinType = WinType.Neutral;

    [DataField("winConditions")]
    public List<WinCondition> WinConditions = new ();

    [DataField("revPlayers")]
    public readonly List<IPlayerSession> RevPlayers = new();

    [DataField("revHeadPlayers")]
    public readonly List<IPlayerSession> RevHeadPlayers = new ();

    [DataField("headPlayers")]
    public readonly List<IPlayerSession> HeadPlayers = new();

    public int TotalHeadRevs => RevHeadPlayers.Count;

    [DataField("backupGameRuleProto", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BackupGameRuleProto = "Traitor";

    [DataField("operativeRoleProto", customTypeSerializer: typeof(PrototypeIdSerializer<AntagPrototype>))]
    public string RevaRoleProto = "HeadRev";

    [DataField("revStartingItems", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> StartingItems = new();

    [DataField("revaPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<GamePresetPrototype>))]
    public static string RevaGamePresetPrototype = "Revolution";

    [DataField("nextTimeCheckConditions")]
    public TimeSpan NextTimeCheckConditions = TimeSpan.Zero;

    public readonly Dictionary<string, bool> ScoreRevPlayers = new();

    public readonly List<string> ScoreHeadPlayers = new();
}

public enum WinType
{
    /// <summary>
    ///     Rev major win. All Crew Heads are dead.
    /// </summary>
    RevMajor,
    /// <summary>
    ///     Neutral win. Shuttle has arrived on CentComm with some Head Revs and some Crew Heads alive.
    /// </summary>
    Neutral,
    /// <summary>
    ///     Crew major win. This means all Head Revs are dead.
    /// </summary>
    CrewMajor
}

public enum WinCondition
{
    AllHeadRevsDead,
    AllCrewHeadsDead
}
