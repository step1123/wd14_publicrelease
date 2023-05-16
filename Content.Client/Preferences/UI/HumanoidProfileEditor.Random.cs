using System.Linq;
using Content.Shared.Dataset;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client.Preferences.UI
{
    public sealed partial class HumanoidProfileEditor
    {
        private readonly IRobustRandom _random;
        private readonly IPrototypeManager _prototypeManager;

        private void RandomizeEverything()
        {
            var species = _prototypeManager.EnumeratePrototypes<SpeciesPrototype>();

            //Пиздец
            var ignoredSpecies = species.Except(_speciesList).Select(x=> x.ID).ToHashSet();

            Profile = HumanoidCharacterProfile.Random(ignoredSpecies);
            UpdateControls();
            IsDirty = true;
        }

        private void RandomizeName()
        {
            if (Profile == null) return;
            var name = HumanoidCharacterProfile.GetName(Profile.Species, Profile.Gender);
            SetName(name);
            UpdateNamesEdit();
        }

        private void RandomizeClownName()
        {
            if (Profile == null) return;
            var name = HumanoidCharacterProfile.GetClownName();
            SetClownName(name);
            UpdateNamesEdit();
        }

        private void RandomizeMimeName()
        {
            if (Profile == null) return;
            var name = HumanoidCharacterProfile.GetMimeName();
            SetMimeName(name);
            UpdateNamesEdit();
        }

        private void RandomizeBorgName()
        {
            if (Profile == null) return;
            var name = HumanoidCharacterProfile.GetBorgName();
            SetBorgName(name);
            UpdateNamesEdit();
        }
    }
}
