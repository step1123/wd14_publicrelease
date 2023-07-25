using Content.Server.GameTicking.Presets;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.White.Cult;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.White.Cult.GameRule;

[RegisterComponent]
public sealed class CultRuleComponent : Component
{

    public readonly SoundSpecifier GreatingsSound = new SoundPathSpecifier("/Audio/White/Cult/blood_cult_greeting.ogg");

    [DataField("cultPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<GamePresetPrototype>))]
    public static string CultGamePresetPrototype = "Cult";

    [DataField("cultistPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<AntagPrototype>))]
    public static string CultistPrototypeId = "Cultist";

    [DataField("reaperPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public static string ReaperPrototype = "ReaperConstruct";

    [ViewVariables(VVAccess.ReadOnly), DataField("tileId")]
    public static string CultFloor = "CultFloor";

    [DataField("eyeColor")]
    public static Color EyeColor = Color.FromHex("#f80000");

    [DataField("redEyeThreshold")]
    public static int ReadEyeThreshold = 5;

    [DataField("pentagramThreshold")]
    public static int PentagramThreshold = 8;

    public Dictionary<IPlayerSession, HumanoidCharacterProfile> StarCandidates = new();

    [DataField("cultistStartingItems", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> StartingItems = new();

    /// <summary>
    ///     Players who played as an cultist at some point in the round.
    /// </summary>
    public Dictionary<string, string> CultistsList = new();

    public Mind.Mind? CultTarget;

    public List<CultistComponent> Cultists = new();

    public CultWinCondition WinCondition;
}

public enum CultWinCondition : byte
{
    CultWin,
    CultFailure
}

public sealed class CultNarsieSummoned : EntityEventArgs
{
}
