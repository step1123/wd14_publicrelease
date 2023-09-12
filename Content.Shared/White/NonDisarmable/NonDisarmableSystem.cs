using Content.Shared.Actions.Events;
using Content.Shared.White.Disarmable;

namespace Content.Shared.White.NonDisarmable;

public sealed class NonDisarmableSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<NonDisarmableComponent,DisarmAttemptEvent>(OnDisarmAttempt);
    }

    private void OnDisarmAttempt(EntityUid uid, NonDisarmableComponent component, DisarmAttemptEvent args)
    {
        args.Cancel();
    }
}
