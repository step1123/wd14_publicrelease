using Robust.Shared.Serialization;
using Content.Shared.FixedPoint;
namespace Content.Shared.Botany;

/// <summary>
///     On interacting with an entity retrieves the entity UID for use with getting the current SeedData of the seed.
/// </summary>
[Serializable, NetSerializable]
public sealed class SeedAnalyzerScannedUserMessage : BoundUserInterfaceMessage
{
    public readonly EntityUid? TargetEntity;
    public int? Yield;
    public float? Production;
    public float? Lifespan;
    public float? Maturation;
    public float? Endurance;
    public float? Potency;
    public bool? Viable;
    public bool? TurnIntoKudzu;
    public bool? Seedless;
    public Dictionary<string, FixedPoint2>? Chemicals;
    public string? DisplayName;
    public bool? CanScream;
    public bool? Slip;
    public bool? Sentient;
    public bool? Ligneous;
    public bool? Bioluminescent;


    public SeedAnalyzerScannedUserMessage(EntityUid? targetEntity,
    int? yield,
    float? production,
    float? lifespan,
    float? maturation,
    float? endurance,
    float? potency,
    bool? viable,
    bool? turnIntoKudzu,
    bool? seedless,
    string? displayName,
    Dictionary<string, FixedPoint2>? chemicals,
    bool? ligneous,
    bool? canScream,
    bool? slip,
    bool? bioluminescent,
    bool? sentient)
    {
        TargetEntity = targetEntity;
        DisplayName = displayName; //broken for unknown reason
        Viable = viable;
        TurnIntoKudzu = turnIntoKudzu;

        // general traits
        Yield = yield;
        Production = production;
        Endurance = endurance;
        Chemicals = chemicals;


        Potency = potency;
        Lifespan = lifespan;
        Maturation = maturation;
        Seedless = seedless;
        Endurance = endurance;
        Ligneous = ligneous;

        // minor traits
        //ConsumeGasses = consumeGasses;
        //ExudeGasses = exudeGasses;
        //NutrientConsumption = nutrientConsumption;
        //WaterConsumption = waterConsumption;
        //IdealHeat = idealHeat;
        //LowPressureTolerance = lowPressureTolerance;
        //HighPressureTolerance = highPressureTolerance;

        // mutations
        CanScream = canScream;
        Bioluminescent = bioluminescent;

    }
}
