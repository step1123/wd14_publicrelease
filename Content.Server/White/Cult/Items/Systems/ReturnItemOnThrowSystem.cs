using Content.Server.Hands.Systems;
using Content.Server.Stunnable;
using Content.Server.White.Cult.Items.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;
using Content.Shared.White.Cult;

namespace Content.Server.White.Cult.Items.Systems;

public sealed class ReturnItemOnThrowSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReturnItemOnThrowComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnThrowHit(EntityUid uid, ReturnItemOnThrowComponent component, ThrowDoHitEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
            return;

        if (!HasComp<MobStateComponent>(args.Target))
            return;

        _stun.TryParalyze(args.Target, TimeSpan.FromSeconds(component.StunTime), true);
        _hands.PickupOrDrop(args.User.Value, uid);
    }
}
