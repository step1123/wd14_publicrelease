using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.White.Cyborg.Laws;

[GenerateTypedNameReferences]
public sealed partial class LawUIContainer : PanelContainer
{
    private readonly Law _law;

    public LawUIContainer(Law law)
    {
        RobustXamlLoader.Load(this);
        _law = law;
        OnStateLawToggled(_OnStateLawPressed);
    }

    public void SetHeading(string desc)
    {
        Title.Text = desc;
    }

    public void SetDescription(string desc)
    {
        Description.SetMessage(desc);
    }

    private void _OnStateLawPressed(BaseButton.ButtonEventArgs args)
    {
        _law.Enabled = StateLaw.Pressed;
    }

    public void OnStateLawToggled(Action<BaseButton.ButtonEventArgs> func)
    {
        StateLaw.OnToggled += func;
    }
}
