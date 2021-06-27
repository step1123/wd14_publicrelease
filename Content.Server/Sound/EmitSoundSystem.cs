using Content.Shared.Audio;
using Content.Shared.Interaction;
using Content.Shared.Throwing;
using Content.Server.Interaction.Components;
using Content.Server.Sound;
using Content.Server.Throwing;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Log;


namespace Content.Server.GameObjects.EntitySystems
{
    [UsedImplicitly]
    public class EmitSoundSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<EmitSoundOnLandComponent, LandEvent>((eUI, comp, arg) => PlaySound(comp));
            SubscribeLocalEvent<EmitSoundOnUseComponent, UseInHandEvent>((eUI, comp, arg) => PlaySound(comp));
            SubscribeLocalEvent<EmitSoundOnThrowComponent, ThrownEvent>((eUI, comp, arg) => PlaySound(comp));
            SubscribeLocalEvent<EmitSoundOnActivateComponent, ActivateInWorldEvent>((eUI, comp, args) => PlaySound(comp));
        }

        private void PlaySound(BaseEmitSoundComponent component)
        {
            if (!string.IsNullOrWhiteSpace(component._soundCollectionName))
            {
                PlayRandomSoundFromCollection(component);
            }
            else if(!string.IsNullOrWhiteSpace(component._soundName))
            {
                PlaySingleSound(component._soundName, component);
            }
            else
            {
                Logger.Warning($"{nameof(component)} Uid:{component.Owner.Uid} has neither {nameof(component._soundCollectionName)} nor {nameof(component._soundName)} to play.");
            }
        }

        private void PlayRandomSoundFromCollection(BaseEmitSoundComponent component)
        {
            if (!string.IsNullOrWhiteSpace(component._soundCollectionName))
            {
                var file = SelectRandomSoundFromSoundCollection(component._soundCollectionName);
                PlaySingleSound(file, component);
            }
        }

        private string SelectRandomSoundFromSoundCollection(string soundCollectionName)
        {
            var soundCollection = _prototypeManager.Index<SoundCollectionPrototype>(soundCollectionName);
            return _random.Pick(soundCollection.PickFiles);
        }

        private static void PlaySingleSound(string soundName, BaseEmitSoundComponent component)
        {
            if (string.IsNullOrWhiteSpace(soundName))
            {
                return;
            }

            if (component._pitchVariation > 0.0)
            {
                SoundSystem.Play(Filter.Pvs(component.Owner), soundName, component.Owner,
                                 AudioHelpers.WithVariation(component._pitchVariation).WithVolume(-2f));
            }
            else
            {
                SoundSystem.Play(Filter.Pvs(component.Owner), soundName, component.Owner,
                                 AudioParams.Default.WithVolume(-2f));
            }
        }
    }
}

