using Content.Server.Power.Components;
using Content.Shared.Hands.Components;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Content.Shared.White.Cyborg.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgHandsSystem : SharedCyborgHandSystem
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(1);
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private TimeSpan _nextUpdateTime;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyborgComponent, CyborgInstrumentSelectedMessage>(OnInstrumentSelect);
        SubscribeLocalEvent<CyborgComponent, BatteryLowEvent>(OnBatteryLow);
        SubscribeLocalEvent<CyborgInstrumentComponent, ContainerGettingRemovedAttemptEvent>(OnUnequipHand);
    }


    private void OnUnequipHand(EntityUid uid, CyborgInstrumentComponent component,
        ContainerGettingRemovedAttemptEvent args)
    {
        args.Cancel();
        TryInsertInstrument(component.CyborgUid, uid);
        _cyborg.UpdateUserInterface(component.CyborgUid);
    }


    private void OnBatteryLow(EntityUid uid, CyborgComponent component, BatteryLowEvent args)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent))
            return;
        foreach (var hand in handsComponent.Hands)
        {
            if (hand.Value.HeldEntity != null)
                TryInsertInstrument(uid, hand.Value.HeldEntity.Value, component);
        }

        _cyborg.UpdateUserInterface(uid, component);
    }


    private void OnInstrumentSelect(EntityUid uid, CyborgComponent component, CyborgInstrumentSelectedMessage args)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent))
            return;

        if (component.Freeze || handsComponent.ActiveHand == null || component.Energy <= 0)
            return;

        if (handsComponent.ActiveHand.HeldEntity != null)
            TryInsertInstrument(uid, handsComponent.ActiveHand.HeldEntity.Value, component);

        if (args.SelectedInstrumentUid != EntityUid.Invalid)
            TryPickupInstrument(uid, args.SelectedInstrumentUid, component, handsComponent);

        _cyborg.UpdateUserInterface(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextUpdateTime)
            return;
        _nextUpdateTime += Delay;

        var query = EntityQueryEnumerator<CyborgInstrumentComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var batteryUid = component.BatteryUid;
            if (TryComp<BatteryComponent>(uid, out _))
                batteryUid = uid;

            if (!batteryUid.HasValue)
                continue;

            _cyborg.TransferEnergy(component.CyborgUid, batteryUid.Value, 20);
        }
    }
}
