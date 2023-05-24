using Content.Server.Speech;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.White.Other.Speech;

public sealed class VoiceOfGodSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoiceOfGodComponent, AccentGetEvent>(OnAccent);
    }

    private string Accentuate(VoiceOfGodComponent component, string message)
    {
        if (!string.IsNullOrEmpty(component.Sound))
        {
            SoundSystem.Play(component.Sound,
                Filter.Pvs(component.Owner, component.SoundRange),
                component.Owner,
                new AudioParams()
                {
                    Volume = component.Volume
                }
            );
        }

        return component.Accent ? message.ToUpper() : message;
    }

    private void OnAccent(EntityUid uid, VoiceOfGodComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(component, args.Message);
    }
}
