using Content.Client.UserInterface.Controls;
using Content.Shared.Radio;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.Radio.Ui;

[GenerateTypedNameReferences]
public sealed partial class IntercomMenu : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public event Action<bool>? OnMicPressed;
    public event Action<bool>? OnSpeakerPressed;
    public event Action<string>? OnChannelSelected;

    private readonly List<string> _channels = new();

    public IntercomMenu()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        MicButton.OnPressed += args => OnMicPressed?.Invoke(args.Button.Pressed);
        SpeakerButton.OnPressed += args => OnSpeakerPressed?.Invoke(args.Button.Pressed);
    }

    public void Update(IntercomBoundUIState state)
    {
        MicButton.Pressed = state.MicEnabled;
        SpeakerButton.Pressed = state.SpeakerEnabled;

        ChannelOptions.Clear();
        _channels.Clear();
        for (var i = 0; i < state.AvailableChannels.Count; i++)
        {
            var channel = state.AvailableChannels[i];
            if (!_prototype.TryIndex<RadioChannelPrototype>(channel, out var prototype))
                continue;

            _channels.Add(channel);
            ChannelOptions.AddItem(Loc.GetString(prototype.Name), i);

            if (channel == state.SelectedChannel)
                ChannelOptions.Select(i);
        }
        ChannelOptions.OnItemSelected += args =>
        {
            ChannelOptions.SelectId(args.Id);
            OnChannelSelected?.Invoke(_channels[args.Id]);
        };
    }
}

