using System.Diagnostics.Contracts;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client.White.UserInterface.Controls;

public class ChatRichTextLabel : Control
{
    [Dependency] private readonly MarkupTagManager _tagManager = default!;

    private FormattedMessage? _message;
    private ChatRichTextEntry _entry;

    public ChatRichTextLabel()
    {
        IoCManager.InjectDependencies(this);
    }

    public void SetMessage(FormattedMessage message)
    {
        _message = message;
        _entry = new ChatRichTextEntry(_message, this, _tagManager);
        InvalidateMeasure();
    }

    public void SetMessage(string message)
    {
        var msg = new FormattedMessage();
        msg.AddText(message);
        SetMessage(msg);
    }

    public string? GetMessage() => _message?.ToMarkup();

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (_message == null)
        {
            return Vector2.Zero;
        }

        var font = _getFont();
        _entry.Update(font, availableSize.X * UIScale, UIScale);

        return new Vector2(_entry.Width / UIScale, _entry.Height / UIScale);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (_message == null)
        {
            return;
        }

        _entry.Draw(handle, _getFont(), SizeBox, 0, new MarkupDrawingContext(), UIScale);
    }

    [Pure]
    private Font _getFont()
    {
        if (TryGetStyleProperty<Font>("font", out var font))
        {
            return font;
        }

        return UserInterfaceManager.ThemeDefaults.DefaultFont;
    }
}
