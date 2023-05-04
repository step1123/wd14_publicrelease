using System.Linq;
using Content.Client.White.Sponsors;
using Content.Client.White.TTS;
using Content.Shared.Preferences;
using Content.Shared.White.TTS;
using Robust.Shared.Random;

namespace Content.Client.Preferences.UI;

public sealed partial class HumanoidProfileEditor
{
    private TTSManager _ttsMgr = default!;
    private TTSSystem _ttsSys = default!;
    private List<TTSVoicePrototype> _voiceList = default!;
    private readonly List<string> _sampleText = new()
    {
        "Съешь же ещё этих мягких французских булок, да выпей чаю.",
        "Клоун, прекрати разбрасывать банановые кожурки офицерам под ноги!",
        "Капитан, вы уверены что хотите назначить клоуна на должность главы персонала?",
        "Эс Бэ! Тут человек в сером костюме, с тулбоксом и в маске! Помогите!!"
    };

    private void InitializeVoice()
    {
        _ttsMgr = IoCManager.Resolve<TTSManager>();
        _ttsSys = _entMan.System<TTSSystem>();
        _voiceList = _prototypeManager.EnumeratePrototypes<TTSVoicePrototype>().Where(o => o.RoundStart).ToList();

        _voiceButton.OnItemSelected += args =>
        {
            _voiceButton.SelectId(args.Id);
            SetVoice(_voiceList[args.Id].ID);
        };

        _voicePlayButton.OnPressed += _ => { PlayTTS(); };

    }

    private void UpdateTTSVoicesControls()
    {
        if (Profile is null)
            return;

        var sponsorsManager = IoCManager.Resolve<SponsorsManager>();

        _voiceButton.Clear();

        var firstVoiceChoiceId = 1;
        for (var i = 0; i < _voiceList.Count; i++)
        {
            var voice = _voiceList[i];
            if (!HumanoidCharacterProfile.CanHaveVoice(voice, Profile.Sex))
                continue;

            var name = Loc.GetString(voice.Name);
            _voiceButton.AddItem(name, i);

            if (firstVoiceChoiceId == 1)
                firstVoiceChoiceId = i;

            if (voice.SponsorOnly &&
                sponsorsManager.TryGetInfo(out var sponsor) &&
                !sponsor.AllowedMarkings.Contains(voice.ID))
            {
                _voiceButton.SetItemDisabled(i, true);
            }
        }

        var voiceChoiceId = _voiceList.FindIndex(x => x.ID == Profile.Voice);
        if (!_voiceButton.TrySelectId(voiceChoiceId) &&
            _voiceButton.TrySelectId(firstVoiceChoiceId))
        {
            SetVoice(_voiceList[firstVoiceChoiceId].ID);
        }
    }

    private void PlayTTS()
    {
        if (_previewDummy is null || Profile is null)
            return;

        _ttsSys.StopAllStreams();
        _ttsMgr.RequestTTS(_previewDummy.Value, _random.Pick(_sampleText), Profile.Voice);
    }
}
