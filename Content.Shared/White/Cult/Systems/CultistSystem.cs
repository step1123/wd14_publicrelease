namespace Content.Shared.White.Cult.Systems;

/// <summary>
/// Thats need for chat perms update
/// </summary>
public sealed class CultistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultistComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<CultistComponent, ComponentShutdown>(OnRemove);
    }

    private void OnInit(EntityUid uid, CultistComponent component, ComponentStartup args)
    {
        RaiseLocalEvent(new EventCultistComponentState(true));
    }

    private void OnRemove(EntityUid uid, CultistComponent component, ComponentShutdown args)
    {
        RaiseLocalEvent(new EventCultistComponentState(false));
    }
}

public sealed class EventCultistComponentState
{
    public bool Created { get; }
    public EventCultistComponentState(bool state)
    {
        Created = state;
    }
}
