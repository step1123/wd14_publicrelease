using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.White.Crossbow;

namespace Content.Server.White.Crossbow;

public sealed class ThrowDamageModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrowDamageModifierComponent, StopThrowEvent>(OnStopped);
        SubscribeLocalEvent<ThrowDamageModifierComponent, EmbedStartEvent>(OnEmbedStart);
        SubscribeLocalEvent<ThrowDamageModifierComponent, EmbedRemovedEvent>(OnEmbedRemoved);
    }

    private void OnEmbedStart(EntityUid uid, ThrowDamageModifierComponent component, ref EmbedStartEvent args)
    {
        component.ClearDamageOnRemove = true;

        if (component.AddEmbedding)
            args.Embed.PreventEmbedding = true;
    }

    private void OnEmbedRemoved(EntityUid uid, ThrowDamageModifierComponent component, EmbedRemovedEvent args)
    {
        if (!component.ClearDamageOnRemove)
            return;

        component.ClearDamageOnRemove = false;
        component.Damage.DamageDict.Clear();

        if (component.AddEmbedding)
            RemComp<EmbeddableProjectileComponent>(uid);
    }

    private void OnStopped(EntityUid uid, ThrowDamageModifierComponent component, StopThrowEvent args)
    {
        if (!component.ClearDamageOnRemove)
            component.Damage.DamageDict.Clear();
    }
}
