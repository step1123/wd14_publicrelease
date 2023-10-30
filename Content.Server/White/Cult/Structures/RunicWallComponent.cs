namespace Content.Server.White.Cult.Structures;

[RegisterComponent]
public sealed class RunicWallComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public string UsedItemID = "RitualDagger";

    [ViewVariables(VVAccess.ReadOnly)]
    public string DropItemID = "CultRunicMetal1";

    [ViewVariables(VVAccess.ReadOnly)]
    public string SpawnStructureID = "CultGirder";
}
