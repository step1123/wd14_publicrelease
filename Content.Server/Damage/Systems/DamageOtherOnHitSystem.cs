using Content.Server.Administration.Logs;
using Content.Server.Damage.Components;
using Content.Server.White.Crossbow;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;

namespace Content.Server.Damage.Systems
{
    public sealed class DamageOtherOnHitSystem : EntitySystem
    {
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger= default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<DamageOtherOnHitComponent, ThrowDoHitEvent>(OnDoHit);
        }

        private void OnDoHit(EntityUid uid, DamageOtherOnHitComponent component, ThrowDoHitEvent args)
        {
            // WD EDIT START
            if (args.Handled)
                return;

            var damage = component.Damage;

            if (TryComp(uid, out ThrowDamageModifierComponent? modifier))
                damage += modifier.Damage;

            var dmg = _damageableSystem.TryChangeDamage(args.Target, damage, component.IgnoreResistances, origin: args.User);
            // WD EDIT END

            // Log damage only for mobs. Useful for when people throw spears at each other, but also avoids log-spam when explosions send glass shards flying.
            if (dmg != null && HasComp<MobStateComponent>(args.Target))
                _adminLogger.Add(LogType.ThrowHit, $"{ToPrettyString(args.Target):target} received {dmg.Total:damage} damage from collision");
        }
    }
}
