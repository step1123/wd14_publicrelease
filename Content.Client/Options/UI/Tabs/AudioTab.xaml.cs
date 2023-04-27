using Content.Shared.CCVar;
using Content.Shared.White;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared;
using Robust.Shared.Configuration;
using Range = Robust.Client.UserInterface.Controls.Range;

namespace Content.Client.Options.UI.Tabs
{
    [GenerateTypedNameReferences]
    public sealed partial class AudioTab : Control
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IClydeAudio _clydeAudio = default!;

        public AudioTab()
        {
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);

            LobbyMusicCheckBox.Pressed = _cfg.GetCVar(CCVars.LobbyMusicEnabled);
            RestartSoundsCheckBox.Pressed = _cfg.GetCVar(CCVars.RestartSoundsEnabled);
            EventMusicCheckBox.Pressed = _cfg.GetCVar(CCVars.EventMusicEnabled);
            AdminSoundsCheckBox.Pressed = _cfg.GetCVar(CCVars.AdminSoundsEnabled);
            StationAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.StationAmbienceEnabled);
            SpaceAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.SpaceAmbienceEnabled);

            ApplyButton.OnPressed += OnApplyButtonPressed;
            ResetButton.OnPressed += OnResetButtonPressed;
            MasterVolumeSlider.OnValueChanged += OnMasterVolumeSliderChanged;
            MidiVolumeSlider.OnValueChanged += OnMidiVolumeSliderChanged;
            AmbienceVolumeSlider.OnValueChanged += OnAmbienceVolumeSliderChanged;
            AmbienceSoundsSlider.OnValueChanged += OnAmbienceSoundsSliderChanged;
            LobbyVolumeSlider.OnValueChanged += OnLobbyVolumeSliderChanged;
            TtsVolumeSlider.OnValueChanged += OnTtsVolumeSliderChanged;
            LobbyMusicCheckBox.OnToggled += OnLobbyMusicCheckToggled;
            RestartSoundsCheckBox.OnToggled += OnRestartSoundsCheckToggled;
            EventMusicCheckBox.OnToggled += OnEventMusicCheckToggled;
            AdminSoundsCheckBox.OnToggled += OnAdminSoundsCheckToggled;
            StationAmbienceCheckBox.OnToggled += OnStationAmbienceCheckToggled;
            SpaceAmbienceCheckBox.OnToggled += OnSpaceAmbienceCheckToggled;

            AmbienceSoundsSlider.MinValue = _cfg.GetCVar(CCVars.MinMaxAmbientSourcesConfigured);
            AmbienceSoundsSlider.MaxValue = _cfg.GetCVar(CCVars.MaxMaxAmbientSourcesConfigured);

            Reset();
        }

        protected override void Dispose(bool disposing)
        {
            ApplyButton.OnPressed -= OnApplyButtonPressed;
            ResetButton.OnPressed -= OnResetButtonPressed;
            MasterVolumeSlider.OnValueChanged -= OnMasterVolumeSliderChanged;
            MidiVolumeSlider.OnValueChanged -= OnMidiVolumeSliderChanged;
            AmbienceVolumeSlider.OnValueChanged -= OnAmbienceVolumeSliderChanged;
            LobbyVolumeSlider.OnValueChanged -= OnLobbyVolumeSliderChanged;

            //WD-EDIT
            TtsVolumeSlider.OnValueChanged -= OnTtsVolumeSliderChanged;
            //WD-EDIT

            base.Dispose(disposing);
        }


        //TTS-Start
        private void OnTtsVolumeSliderChanged(Range obj)
        {
            UpdateChanges();
        }
        //TTS-End

        private void OnLobbyVolumeSliderChanged(Range obj)
        {
            UpdateChanges();
        }

        private void OnAmbienceVolumeSliderChanged(Range obj)
        {
            UpdateChanges();
        }

        private void OnAmbienceSoundsSliderChanged(Range obj)
        {
            UpdateChanges();
        }

        private void OnMasterVolumeSliderChanged(Range range)
        {
            _clydeAudio.SetMasterVolume(MasterVolumeSlider.Value / 100);
            UpdateChanges();
        }

        private void OnMidiVolumeSliderChanged(Range range)
        {
            UpdateChanges();
        }

        private void OnLobbyMusicCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }
        private void OnRestartSoundsCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }
        private void OnEventMusicCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }

        private void OnAdminSoundsCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }

        private void OnStationAmbienceCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }

        private void OnSpaceAmbienceCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }

        private void OnApplyButtonPressed(BaseButton.ButtonEventArgs args)
        {
            _cfg.SetCVar(CVars.AudioMasterVolume, MasterVolumeSlider.Value / 100);
            _cfg.SetCVar(CVars.MidiVolume, LV100ToDB(MidiVolumeSlider.Value));
            _cfg.SetCVar(CCVars.AmbienceVolume, LV100ToDB(AmbienceVolumeSlider.Value));
            _cfg.SetCVar(CCVars.LobbyMusicVolume, LV100ToDB(LobbyVolumeSlider.Value));
            _cfg.SetCVar(CCVars.MaxAmbientSources, (int)AmbienceSoundsSlider.Value);
            _cfg.SetCVar(CCVars.LobbyMusicEnabled, LobbyMusicCheckBox.Pressed);
            _cfg.SetCVar(CCVars.RestartSoundsEnabled, RestartSoundsCheckBox.Pressed);
            _cfg.SetCVar(CCVars.EventMusicEnabled, EventMusicCheckBox.Pressed);
            _cfg.SetCVar(CCVars.AdminSoundsEnabled, AdminSoundsCheckBox.Pressed);
            _cfg.SetCVar(CCVars.StationAmbienceEnabled, StationAmbienceCheckBox.Pressed);
            _cfg.SetCVar(CCVars.SpaceAmbienceEnabled, SpaceAmbienceCheckBox.Pressed);

            //WD-EDIT
            _cfg.SetCVar(WhiteCVars.TtsVolume, LV100ToDB(TtsVolumeSlider.Value));
            //WD-EDIT


            _cfg.SaveToFile();
            UpdateChanges();
        }

        private void OnResetButtonPressed(BaseButton.ButtonEventArgs args)
        {
            Reset();
        }

        private void Reset()
        {
            MasterVolumeSlider.Value = _cfg.GetCVar(CVars.AudioMasterVolume) * 100;
            MidiVolumeSlider.Value = DBToLV100(_cfg.GetCVar(CVars.MidiVolume));
            AmbienceVolumeSlider.Value = DBToLV100(_cfg.GetCVar(CCVars.AmbienceVolume));
            LobbyVolumeSlider.Value = DBToLV100(_cfg.GetCVar(CCVars.LobbyMusicVolume));
            AmbienceSoundsSlider.Value = _cfg.GetCVar(CCVars.MaxAmbientSources);
            LobbyMusicCheckBox.Pressed = _cfg.GetCVar(CCVars.LobbyMusicEnabled);
            RestartSoundsCheckBox.Pressed = _cfg.GetCVar(CCVars.RestartSoundsEnabled);
            EventMusicCheckBox.Pressed = _cfg.GetCVar(CCVars.EventMusicEnabled);
            AdminSoundsCheckBox.Pressed = _cfg.GetCVar(CCVars.AdminSoundsEnabled);
            StationAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.StationAmbienceEnabled);
            SpaceAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.SpaceAmbienceEnabled);

            //WD-EDIT
            TtsVolumeSlider.Value = DBToLV100(_cfg.GetCVar(WhiteCVars.TtsVolume));
            //WD-EDIT


            UpdateChanges();
        }

        // Note: Rather than moving these functions somewhere, instead switch MidiManager to using linear units rather than dB
        // Do be sure to rename the setting though
        private float DBToLV100(float db)
        {
            return (MathF.Pow(10, (db / 10)) * 100);
        }

        private float LV100ToDB(float lv100)
        {
            // Saving negative infinity doesn't work, so use -10000000 instead (MidiManager does it)
            return MathF.Max(-10000000, MathF.Log(lv100 / 100, 10) * 10);
        }

        private void UpdateChanges()
        {
            var isMasterVolumeSame =
                Math.Abs(MasterVolumeSlider.Value - _cfg.GetCVar(CVars.AudioMasterVolume) * 100) < 0.01f;
            var isMidiVolumeSame =
                Math.Abs(MidiVolumeSlider.Value - DBToLV100(_cfg.GetCVar(CVars.MidiVolume))) < 0.01f;
            var isAmbientVolumeSame =
                Math.Abs(AmbienceVolumeSlider.Value - DBToLV100(_cfg.GetCVar(CCVars.AmbienceVolume))) < 0.01f;
            var isLobbyVolumeSame =
                Math.Abs(LobbyVolumeSlider.Value - DBToLV100(_cfg.GetCVar(CCVars.LobbyMusicVolume))) < 0.01f;
            var isAmbientSoundsSame = (int)AmbienceSoundsSlider.Value == _cfg.GetCVar(CCVars.MaxAmbientSources);
            var isLobbySame = LobbyMusicCheckBox.Pressed == _cfg.GetCVar(CCVars.LobbyMusicEnabled);

            //WD-EDIT
            var isTtsVolumeSame =
                Math.Abs(TtsVolumeSlider.Value - DBToLV100(_cfg.GetCVar(WhiteCVars.TtsVolume))) < 0.01f;
            //WD-EDIT

            var isRestartSoundsSame = RestartSoundsCheckBox.Pressed == _cfg.GetCVar(CCVars.RestartSoundsEnabled);
            var isEventSame = EventMusicCheckBox.Pressed == _cfg.GetCVar(CCVars.EventMusicEnabled);
            var isAdminSoundsSame = AdminSoundsCheckBox.Pressed == _cfg.GetCVar(CCVars.AdminSoundsEnabled);
            var isStationAmbienceSame = StationAmbienceCheckBox.Pressed == _cfg.GetCVar(CCVars.StationAmbienceEnabled);
            var isSpaceAmbienceSame = SpaceAmbienceCheckBox.Pressed == _cfg.GetCVar(CCVars.SpaceAmbienceEnabled);
            var isEverythingSame = isMasterVolumeSame && isMidiVolumeSame && isAmbientVolumeSame && isAmbientSoundsSame && isLobbySame && isRestartSoundsSame && isEventSame
                                   && isAdminSoundsSame && isStationAmbienceSame && isSpaceAmbienceSame && isLobbyVolumeSame;
            isEverythingSame = isEverythingSame && isTtsVolumeSame; //WD-EDIT
            ApplyButton.Disabled = isEverythingSame;
            ResetButton.Disabled = isEverythingSame;
            MasterVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", MasterVolumeSlider.Value / 100));
            MidiVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", MidiVolumeSlider.Value / 100));
            AmbienceVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", AmbienceVolumeSlider.Value / 100));
            LobbyVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", LobbyVolumeSlider.Value / 100));
            AmbienceSoundsLabel.Text = ((int)AmbienceSoundsSlider.Value).ToString();

            //WD-EDIT
            TtsVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", TtsVolumeSlider.Value / 100));
            //WD-EDIT
        }
    }
}
