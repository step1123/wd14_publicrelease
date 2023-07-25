namespace Content.Server.White.Cult.HolyWater;

[RegisterComponent]
public sealed class BibleWaterConvertComponent : Component
{
    [DataField("convertedId"), ViewVariables(VVAccess.ReadWrite)]
    public string ConvertedId = "Water";

    [DataField("ConvertedToId"), ViewVariables(VVAccess.ReadWrite)]
    public string ConvertedToId = "HolyWater";
}
