using System.Linq;
using Content.Client.Chat.Managers;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Emotions.Windows;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client.UserInterface.Systems.Emotions;

public sealed class EmotionsUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;


    private EmotionsWindow? _window;
    private MenuButton? EmotionsButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.EmotionsButton;

    private DateTime _lastEmotionTimeUse = DateTime.Now;
    private const float EmoteCooldown = 1.5f;

    public void OnStateEntered(GameplayState state)
    {
        _window = UIManager.CreateWindow<EmotionsWindow>();

        _window.OnOpen += OnWindowOpened;
        _window.OnClose += OnWindowClosed;

        var emotions = _prototypeManager.EnumeratePrototypes<EmotePrototype>().ToList();
        emotions.Sort((a,b) => string.Compare(a.ButtonText, b.ButtonText.ToString(), StringComparison.Ordinal));

        foreach (var emote in emotions)
        {
            if (!emote.AllowToEmotionsMenu)
                continue;
            var control = new Button();
            control.OnPressed += _ => UseEmote(_random.Pick(emote.ChatMessages));
            control.Text = emote.ButtonText;
            control.HorizontalExpand = true;
            control.VerticalExpand = true;
            control.MaxWidth = 250;
            control.MaxHeight = 50;
            _window.EmotionsContainer.AddChild(control);
        }

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenEmotionsMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<EmotionsUIController>();
    }

    public void UnloadButton()
    {
        if (EmotionsButton == null)
        {
            return;
        }

        EmotionsButton.OnPressed -= EmotionsButtonPressed;
    }

    private void UseEmote(string emote)
    {
        var timeSpan = DateTime.Now - _lastEmotionTimeUse;
        var seconds = timeSpan.TotalSeconds;
        if (seconds < EmoteCooldown)
        {
            return;
        }

        _lastEmotionTimeUse = DateTime.Now;
        _chatManager.SendMessage(emote, ChatSelectChannel.Emotes);
    }

    public void LoadButton()
    {
        if (EmotionsButton == null)
        {
            return;
        }

        EmotionsButton.OnPressed += EmotionsButtonPressed;
    }

    private void OnWindowOpened()
    {
        if (EmotionsButton != null)
            EmotionsButton.Pressed = true;
    }

    private void OnWindowClosed()
    {
        if (EmotionsButton != null)
            EmotionsButton.Pressed = false;
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.OnOpen -= OnWindowOpened;
            _window.OnClose -= OnWindowClosed;

            _window.Dispose();
            _window = null;
        }

        CommandBinds.Unregister<EmotionsUIController>();
    }

    private void EmotionsButtonPressed(BaseButton.ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void ToggleWindow()
    {
        if (_window == null)
            return;

        if (_window.IsOpen)
        {
            _window.Close();
            return;
        }

        _window.Open();
    }
}
