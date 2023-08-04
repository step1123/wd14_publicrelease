namespace Content.Shared.White.Sales;

[DataDefinition]
public sealed class SalesSpecifier
{
    [DataField("enabled")]
    public bool Enabled { get; }

    [DataField("minMultiplier")]
    public float MinMultiplier { get; }

    [DataField("maxMultiplier")]
    public float MaxMultiplier { get; }

    [DataField("minItems")]
    public int MinItems { get; }

    [DataField("maxItems")]
    public int MaxItems { get; }

    [DataField("salesCategory")]
    public string SalesCategory { get; } = string.Empty;

    public SalesSpecifier()
    {
    }

    public SalesSpecifier(bool enabled, float minMultiplier, float maxMultiplier, int minItems, int maxItems,
        string salesCategory)
    {
        Enabled = enabled;
        MinMultiplier = minMultiplier;
        MaxMultiplier = maxMultiplier;
        MinItems = minItems;
        MaxItems = maxItems;
        SalesCategory = salesCategory;
    }
}
