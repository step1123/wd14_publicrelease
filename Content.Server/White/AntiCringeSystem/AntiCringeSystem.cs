namespace Content.Server.White.AntiCringeSystem;


public sealed class AntiCringeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AntiCringeComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, AntiCringeComponent component, ComponentInit args)
    {
        Del(uid);
    }
}
