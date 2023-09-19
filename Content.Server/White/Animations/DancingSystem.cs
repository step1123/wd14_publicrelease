using Content.Shared.Animations;
using Content.Shared.Bed.Sleep;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Random;

namespace Content.Server.Animations;

public sealed class DancingSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EmoteAnimationSystem _emoteAnimation = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private readonly string[] _emoteList = {"EmoteFlip", "EmoteTurn"};

    public override void Initialize()
    {
        SubscribeLocalEvent<DancingComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, DancingComponent component, ComponentInit args)
    {
        ResetDelay(component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<DancingComponent, EmoteAnimationComponent, MobStateComponent>();

        while (query.MoveNext(out var uid, out var dancing, out var emote, out var mobState))
        {
            if (!_mobState.IsAlive(uid, mobState) || HasComp<SleepingComponent>(uid))
                continue;

            dancing.AccumulatedFrametime += frameTime;

            if (dancing.AccumulatedFrametime < dancing.NextDelay)
                continue;

            dancing.AccumulatedFrametime -= dancing.NextDelay;

            ResetDelay(dancing);

            _emoteAnimation.PlayEmoteAnimation(uid, emote, _random.Pick(_emoteList));
        }
    }

    private void ResetDelay(DancingComponent component)
    {
        component.NextDelay = _random.NextFloat() + 0.2f;
    }
}
