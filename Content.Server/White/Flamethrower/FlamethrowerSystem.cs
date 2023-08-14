using Content.Server.IgnitionSource;
using Content.Shared.Temperature;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Server.White.Flamethrower;

public sealed class FlamethrowerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlamethrowerComponent, GunShotEvent>(OnShoot);
    }

    private void OnShoot(EntityUid uid, FlamethrowerComponent component, ref GunShotEvent args)
    {
        var hasIgnition = TryComp(uid, out IgnitionSourceComponent? ignition);
        var isHotEvent = new IsHotEvent {IsHot = hasIgnition && ignition!.Ignited};

        foreach (var (shotUid, _) in args.Ammo)
        {
            if(shotUid is not null)
                RaiseLocalEvent(shotUid.Value, isHotEvent);
        }
    }
}
