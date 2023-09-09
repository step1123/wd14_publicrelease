using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.MainMenu;

public sealed class BackgroundControl : TextureRect
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private IRenderTexture? _buffer;
    private readonly ShaderInstance _glitchShader;

    public BackgroundControl()
    {
        IoCManager.InjectDependencies(this);

        _glitchShader = _prototype.Index<ShaderPrototype>("Melt").Instance().Duplicate();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _buffer?.Dispose();
    }

    protected override void Resized()
    {
        base.Resized();

        _buffer?.Dispose();
        _buffer = _clyde.CreateRenderTarget(PixelSize, RenderTargetColorFormat.Rgba8Srgb, default);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        if (_buffer is null)
            return;

        handle.RenderInRenderTarget(_buffer, () =>
        {
            base.Draw(handle);
        }, Color.Transparent);

        handle.UseShader(_glitchShader);

        handle.DrawTextureRect(_buffer.Texture, PixelSizeBox);
        handle.UseShader(null);
    }
}
