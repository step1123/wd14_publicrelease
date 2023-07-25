using System.Threading;
using Content.Shared.White.Cult;
using Robust.Server.GameObjects;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

    private void InitializeNarsie()
    {
        SubscribeLocalEvent<NarsieComponent, ComponentInit>(OnNarsieComponentInit);
    }

    private void OnNarsieComponentInit(EntityUid uid, NarsieComponent component, ComponentInit args)
    {
        _appearanceSystem.SetData(uid, NarsieVisualState.VisualState, NarsieVisuals.Spawning);

        Timer.Spawn(TimeSpan.FromSeconds(6), () =>
        {
            _appearanceSystem.SetData(uid, NarsieVisualState.VisualState, NarsieVisuals.Spawned);
        });
    }
}
