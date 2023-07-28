using Content.Shared.White.StackHolder;
using Robust.Client.GameObjects;

namespace Content.Client.White.StackHolder;

public sealed class StackHolderSystem : SharedStackHolderSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<StackChangeEvent>(OnStackChanged);
    }

    private void OnStackChanged(StackChangeEvent ev)
    {
        UpdateSprite(ev.Original, ev.Replica);
    }

    private void UpdateSprite(EntityUid uid, EntityUid other)
    {
        if (!TryComp<SpriteComponent>(uid, out var originalSprite)
            || !TryComp<SpriteComponent>(other, out var replicaSprite))
            return;
        originalSprite.CopyFrom(replicaSprite);
    }
}
