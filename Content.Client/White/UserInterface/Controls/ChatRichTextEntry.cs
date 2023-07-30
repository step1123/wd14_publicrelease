using System.Numerics;
using System.Text;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Collections;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.White.UserInterface.Controls;

internal struct ChatRichTextEntry
{

    private static readonly Color DefaultColor = new(200, 200, 200);

    private readonly MarkupTagManager _tagManager;

    public readonly FormattedMessage Message;

    /// <summary>
    ///     The vertical size of this entry, in pixels.
    /// </summary>
    public int Height;

    /// <summary>
    ///     The horizontal size of this entry, in pixels.
    /// </summary>
    public int Width;

    /// <summary>
    ///     The combined text indices in the message's text tags to put line breaks.
    /// </summary>
    public ValueList<int> LineBreaks;

    private readonly Dictionary<int, Control> _tagControls = new();

    public ChatRichTextEntry(FormattedMessage message, Control parent, MarkupTagManager tagManager)
    {
        Message = message;
        Height = 0;
        Width = 0;
        LineBreaks = default;
        _tagManager = tagManager;

        var nodeIndex = -1;
        foreach (var node in Message.Nodes)
        {
            nodeIndex++;

            if (node.Name == null)
                continue;

            if (!_tagManager.TryGetMarkupTag(node.Name, out var tag) || !tag.TryGetControl(node, out var control))
                continue;

            parent.Children.Add(control);
            _tagControls.Add(nodeIndex, control);
        }
    }

    /// <summary>
    ///     Recalculate line dimensions and where it has line breaks for word wrapping.
    /// </summary>
    /// <param name="defaultFont">The font being used for display.</param>
    /// <param name="maxSizeX">The maximum horizontal size of the container of this entry.</param>
    /// <param name="uiScale"></param>
    public void Update(Font defaultFont, float maxSizeX, float uiScale)
    {
        // This method is gonna suck due to complexity.
        // Bear with me here.
        // I am so deeply sorry for the person adding stuff to this in the future.

        Height = defaultFont.GetHeight(uiScale);
        LineBreaks.Clear();

        int? breakLine;
        var wordWrap = new WordWrap(maxSizeX);
        var context = new MarkupDrawingContext();
        context.Font.Push(defaultFont);
        context.Color.Push(DefaultColor);

        // Go over every node.
        // Nodes can change the markup drawing context and return additional text.
        // It's also possible for nodes to return inline controls. They get treated as one large rune.
        var nodeIndex = -1;
        foreach (var node in Message.Nodes)
        {
            nodeIndex++;
            var text = ProcessNode(node, context);

            if (!context.Font.TryPeek(out var font))
                font = defaultFont;

            // And go over every character.
            foreach (var rune in text.EnumerateRunes())
            {
                if (ProcessRune(ref this, rune, out breakLine))
                    continue;

                // Uh just skip unknown characters I guess.
                if (!font.TryGetCharMetrics(rune, uiScale, out var metrics))
                    continue;

                if (ProcessMetric(ref this, metrics, out breakLine))
                    return;
            }

            if (!_tagControls.TryGetValue(nodeIndex, out var control))
                continue;

            if (ProcessRune(ref this, new Rune(' '), out breakLine))
                continue;

            control.Measure(new Vector2(Width, Height));

            var desiredSize = control.DesiredPixelSize;
            var controlMetrics = new CharMetrics(
                0, 0,
                desiredSize.X,
                desiredSize.X,
                desiredSize.Y);

            if (ProcessMetric(ref this, controlMetrics, out breakLine))
                return;
        }

        Width = wordWrap.FinalizeText(out breakLine);
        CheckLineBreak(ref this, breakLine);

        bool ProcessRune(ref ChatRichTextEntry src, Rune rune, out int? outBreakLine)
        {
            wordWrap.NextRune(rune, out breakLine, out var breakNewLine, out var skip);
            CheckLineBreak(ref src, breakLine);
            CheckLineBreak(ref src, breakNewLine);
            outBreakLine = breakLine;
            return skip;
        }

        bool ProcessMetric(ref ChatRichTextEntry src, CharMetrics metrics, out int? outBreakLine)
        {
            wordWrap.NextMetrics(metrics, out breakLine, out var abort);
            CheckLineBreak(ref src, breakLine);
            outBreakLine = breakLine;
            return abort;
        }

        void CheckLineBreak(ref ChatRichTextEntry src, int? line)
        {
            if (line is { } l)
            {
                src.LineBreaks.Add(l);
                if (!context.Font.TryPeek(out var font))
                    font = defaultFont;

                src.Height += font.GetLineHeight(uiScale);
            }
        }
    }

    public readonly void Draw(
        DrawingHandleScreen handle,
        Font defaultFont,
        UIBox2 drawBox,
        float verticalOffset,
        MarkupDrawingContext context,
        float uiScale)
    {
        context.Clear();
        context.Color.Push(DefaultColor);
        context.Font.Push(defaultFont);

        //handle.UseShader(MakeNewShader(false, 16));

        var globalBreakCounter = 0;
        var lineBreakIndex = 0;
        var baseLine = drawBox.TopLeft + new Vector2(0, defaultFont.GetAscent(uiScale) + verticalOffset);
        var controlYAdvance = 0;

        var nodeIndex = -1;
        foreach (var node in Message.Nodes)
        {
            nodeIndex++;
            var text = ProcessNode(node, context);
            if (!context.Color.TryPeek(out var color) || !context.Font.TryPeek(out var font))
            {
                color = DefaultColor;
                font = defaultFont;
            }

            foreach (var rune in text.EnumerateRunes())
            {
                if (lineBreakIndex < LineBreaks.Count &&
                    LineBreaks[lineBreakIndex] == globalBreakCounter)
                {
                    baseLine = new Vector2(drawBox.Left, baseLine.Y + font.GetLineHeight(uiScale) + controlYAdvance);
                    controlYAdvance = 0;
                    lineBreakIndex += 1;
                }
                // Костыльно
                // Пока поставлю на 0.4. Потом надо адаптировать на 0.5
                // MENTION: Это старый вариант обводки с добавлением задней буквы
                //var baseLineBackground = new Vector2(
                //    baseLine.X - (font.GetLineHeight(uiScale+0.4f)/font.GetLineHeight(uiScale)),
                //    baseLine.Y + (font.GetLineHeight(uiScale+0.4f)/(font.GetLineHeight(uiScale)/2.5f))
                //);
                //font.DrawChar(handle, rune, baseLineBackground, uiScale+0.5f, new Color(0,0,0)); // или 0.4f
                // MENTION2: Новый варик с шейдером. Шейдер кривой, а код грязный
                //handle.UseShader(MakeNewShader()); // Ах да, я же шейдеры оставил в ебанинах ахах

                // Также именно в этом файле пишем приколы со шрифтами.
                // на уровне отрисовки лол.

                //var sprite_icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/job_icons.rsi"), "NoId");
                //var _iconTexture = IoCManager.Resolve<IEntityManager>().EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(sprite_icon);
                var advance = font.DrawChar(handle, rune, baseLine, uiScale, color, true);
                baseLine += new Vector2(advance, 0);
                //handle.DrawTextureRect(_iconTexture, new UIBox2(baseLine += new Vector2(0, 5), new Vector2(6, 6)));
                //baseLine += new Vector2(10, 0);

                globalBreakCounter += 1;
            }

            if (!_tagControls.TryGetValue(nodeIndex, out var control))
                continue;

            // Почему-то не работает лол
            //control.Position = new Vector2(baseLine.X, baseLine.Y - defaultFont.GetAscent(uiScale));
            control.Measure(new Vector2(Width, Height));
            var advanceX = control.DesiredPixelSize.X;
            controlYAdvance = Math.Max(0, control.DesiredPixelSize.Y - font.GetLineHeight(uiScale));
            baseLine += new Vector2(advanceX, 0);
        }
    }

    // Unused shader maker
    //private ShaderInstance MakeNewShader()
    //{
    //    var shaderName = "SelectionOutlineBlack";
//
    //    var instance = IoCManager.Resolve<IPrototypeManager>().Index<ShaderPrototype>(shaderName).InstanceUnique();
    //    //instance.SetParameter("outline_width", 1f);
    //    //instance.SetParameter("SCREEN_TEXTURE", viewport.RenderTarget.Texture);
    //    return instance;
    //}

    private readonly string ProcessNode(MarkupNode node, MarkupDrawingContext context)
    {
        // If a nodes name is null it's a text node.
        if (node.Name == null)
            return node.Value.StringValue ?? "";

        //Skip the node if there is no markup tag for it.
        if (!_tagManager.TryGetMarkupTag(node.Name, out var tag))
            return "";

        if (!node.Closing)
        {
            tag.PushDrawContext(node, context);
            return tag.TextBefore(node);
        }

        tag.PopDrawContext(node, context);
        return tag.TextAfter(node);
    }
}
