using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgChargeFillupSystem : EntitySystem
{
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LimitedChargesComponent, CyborgInstrumentGotPickupEvent>(OnPickup);
        SubscribeLocalEvent<LimitedChargesComponent, CyborgInstrumentGotInsertedEvent>(OnInsert);
    }

    private void OnInsert(EntityUid uid, LimitedChargesComponent component, CyborgInstrumentGotInsertedEvent args)
    {
        RemComp<ActiveChargesChargeComponent>(uid);
    }

    private void OnPickup(EntityUid uid, LimitedChargesComponent component, CyborgInstrumentGotPickupEvent args)
    {
        EnsureComp<ActiveChargesChargeComponent>(uid).NextUpdateTime = _timing.CurTime;
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query =
            EntityQueryEnumerator<CyborgInstrumentComponent, ActiveChargesChargeComponent, LimitedChargesComponent>();
        while (query.MoveNext(out var uid, out var instrumentComponent, out var activeChargesChargeComponent,
                   out var limitedChargesComponent))
        {
            if (_timing.CurTime < activeChargesChargeComponent.NextUpdateTime)
                return;
            activeChargesChargeComponent.NextUpdateTime += activeChargesChargeComponent.UpdateRate;

            if (limitedChargesComponent.Charges == limitedChargesComponent.MaxCharges)
                return;

            _charges.AddCharges(uid, 1);
            _cyborg.TryChangeEnergy(instrumentComponent.CyborgUid, -5);
        }
    }
}
