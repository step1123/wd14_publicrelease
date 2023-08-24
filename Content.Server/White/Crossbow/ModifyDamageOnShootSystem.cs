using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Server.White.Crossbow;

public sealed class ModifyDamageOnShootSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModifyDamageOnShootComponent, AmmoShotEvent>(OnShoot);
    }

    private void OnShoot(EntityUid uid, ModifyDamageOnShootComponent component, AmmoShotEvent args)
    {
        foreach (var proj in args.FiredProjectiles)
        {
            var comp = EnsureComp<ThrowDamageModifierComponent>(proj);
            comp.Damage += component.Damage;

            if (!component.AddEmbedding)
                continue;

            comp.AddEmbedding = true;
            var embed = EnsureComp<EmbeddableProjectileComponent>(proj);
            embed.Offset = component.Offset;
            embed.PreventEmbedding = false;
            Dirty(embed);
        }
    }
}
