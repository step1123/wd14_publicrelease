using Content.Shared.Projectiles;

namespace Content.Shared.White.Crossbow;

[ByRefEvent]
public readonly record struct EmbedStartEvent(EmbeddableProjectileComponent Embed);

public sealed class EmbedRemovedEvent : EntityEventArgs
{
}
