using System.Numerics;

namespace Content.Shared.White.Other.FootPrints;

[RegisterComponent]
public sealed class FootPrintsComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), DataField("leftBarePrint")]
    public string LeftBarePrint = "footprint-left-bare-human";

    [ViewVariables(VVAccess.ReadOnly), DataField("rightBarePrint")]
    public string RightBarePrint { get; } = "footprint-right-bare-human";

    [ViewVariables(VVAccess.ReadOnly), DataField("shoesPrint")]
    public string ShoesPrint = "footprint-shoes";

    [ViewVariables(VVAccess.ReadOnly), DataField("suitPrint")]
    public string SuitPrint = "footprint-suit";

    [ViewVariables(VVAccess.ReadOnly), DataField("draggingPrint")]
    public string[] DraggingPrint = new string[5]
        {
            "dragging-1",
            "dragging-2",
            "dragging-3",
            "dragging-4",
            "dragging-5"
        };

    [ViewVariables(VVAccess.ReadWrite), DataField("offsetCenter")]
    public Vector2 OffsetCenter = new(-0.5f, -1f);

    [ViewVariables(VVAccess.ReadWrite), DataField("offsetPrint")]
    public Vector2 OffsetPrint = new(0.1f, 0f);

    [ViewVariables(VVAccess.ReadOnly), DataField("color")]
    public Color PrintsColor = Color.FromHex("#00000000");

    [ViewVariables(VVAccess.ReadWrite), DataField("stepSize")]
    public float StepSize = 0.7f;

    [ViewVariables(VVAccess.ReadWrite), DataField("dragSize")]
    public float DragSize = 0.5f;
    public bool RightStep = true;
    public Vector2 StepPos = Vector2.Zero;

    [ViewVariables(VVAccess.ReadWrite), DataField("colorQuantity")]
    public float ColorQuantity;

    [ViewVariables(VVAccess.ReadWrite), DataField("colorReduceAlpha")]
    public float ColorReduceAlpha = 0.15f;
}


