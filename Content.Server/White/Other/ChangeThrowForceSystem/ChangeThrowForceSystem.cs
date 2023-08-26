using Content.Shared.Throwing;

namespace Content.Server.White.Other.ChangeThrowForceSystem;

public sealed class ChangeThrowForceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangeThrowForceComponent, BeforeThrowEvent>(HandleThrow);
    }

    private void HandleThrow(EntityUid uid, ChangeThrowForceComponent component, BeforeThrowEvent args)
    {
        args.ThrowStrength = component.ThrowForce;
    }
}
