namespace Content.Server.White.AntiCringeSystem;


public sealed class AntiCringeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AntiCringeComponent, MapInitEvent>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, AntiCringeComponent component, MapInitEvent args)
    {
        QueueDel(uid);
    }
}
