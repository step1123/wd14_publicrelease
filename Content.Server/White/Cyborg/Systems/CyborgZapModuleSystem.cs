using Content.Server.Atmos.Miasma;
using Content.Server.DoAfter;
using Content.Server.Electrocution;
using Content.Server.EUI;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.White.Cyborg.Components;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgZapModuleSystem : EntitySystem
{
    [Dependency] private readonly CyborgSystem _cyborg = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly RottingSystem _rotting = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgZapModuleComponent,ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, CyborgZapModuleComponent component, ExaminedEvent args)
    {
        if(component.IsUsed)
            args.PushMarkup(Loc.GetString("module-zip-used"));
    }

    public void UseZap(EntityUid uid, CyborgModuleComponent? component = null,
        CyborgZapModuleComponent? zapModuleComponent = null)
    {
        if(!Resolve(uid,ref component,ref zapModuleComponent) || zapModuleComponent.IsUsed || !component.Parent.HasValue)
            return;

        var target = component.Parent.Value;
        Logger.Debug($"ZAPPING {target}");
        _mobThreshold.SetAllowRevives(target, true);
        if (_mobState.IsDead(target))
            _damageable.TryChangeDamage(target, zapModuleComponent.ZapHeal, true);
        _mobState.ChangeMobState(target, MobState.Critical);
        _mobThreshold.SetAllowRevives(target, false);

        zapModuleComponent.IsUsed = true;
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CyborgZapModuleComponent, CyborgModuleComponent>();

        while (query.MoveNext(out var uid,out var zapModule,out var module))
        {
            if(!module.Parent.HasValue)
                return;

            if (zapModule.IsUsed)
            {
                _cyborg.RemoveModule(uid,module);
                return;
            }

            if (!_mobState.IsAlive(module.Parent.Value))
            {
                UseZap(uid,module,zapModule);
            }


        }
    }
}
