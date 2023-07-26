using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.White.Cyborg.SiliconBrain.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.SiliconBrain.Systems;

public abstract class SharedPositronicBrainSystem : EntitySystem
{
    [Serializable]
    [NetSerializable]
    public enum PosiStatus : byte
    {
        Standby,
        Searching,
        Occupied
    }


    [Serializable]
    [NetSerializable]
    public enum PosiVisuals : byte
    {
        Status
    }

    public override void Initialize()
    {
        SubscribeLocalEvent<PositronicBrainComponent, UseAttemptEvent>(OnChancel);
        SubscribeLocalEvent<PositronicBrainComponent, InteractionAttemptEvent>(OnChancel);
        SubscribeLocalEvent<PositronicBrainComponent, DropAttemptEvent>(OnChancel);
        SubscribeLocalEvent<PositronicBrainComponent, PickupAttemptEvent>(OnChancel);
        SubscribeLocalEvent<PositronicBrainComponent, UpdateCanMoveEvent>(OnChancel);
        SubscribeLocalEvent<PositronicBrainComponent, ChangeDirectionAttemptEvent>(OnChancel);
    }

    private void OnChancel(EntityUid uid, PositronicBrainComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }
}
