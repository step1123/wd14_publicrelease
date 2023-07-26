using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgKaboomSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgComponent, CyborgActionSelectedEvent>(OnActionSelected);
    }

    private void OnActionSelected(EntityUid uid, CyborgComponent component, CyborgActionSelectedEvent args)
    {
        if (args.User == null)
            return;

        if (args.Action is CyborgActionKey.Blow && _cyborg.HasAccess(uid, args.User))
        {
            _adminLog.Add(LogType.Explosion, LogImpact.Extreme,
                $"{ToPrettyString(uid):player} has blow from console by {ToPrettyString(args.User.Value):player}");

            _explosion.QueueExplosion(uid, ExplosionSystem.DefaultExplosionPrototypeId, 4, 1, 2, maxTileBreak: 0);
            _body.GibBody(uid);
        }

        if (args.Action is CyborgActionKey.Freeze && _cyborg.HasAccess(uid, args.User))
        {
            var isFreeze = component.Freeze ? "unfreeze" : "freeze";
            _adminLog.Add(LogType.Action, LogImpact.High,
                $"{ToPrettyString(uid):player} has {isFreeze} from console by {ToPrettyString(args.User.Value):player}");

            // BatteryLowEvent because at this event, the tools are put back into the borg
            if (component.BatterySlot.ContainedEntity != null)
            {
                var ev = new BatteryLowEvent(uid, component.BatterySlot.ContainedEntity.Value);
                RaiseLocalEvent(uid, ev);
            }

            component.Freeze = !component.Freeze;
            _actionBlocker.UpdateCanMove(uid);
        }
    }
}
