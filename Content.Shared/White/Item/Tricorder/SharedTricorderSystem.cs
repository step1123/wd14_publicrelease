using Robust.Shared.GameStates;

namespace Content.Shared.White.Item.Tricorder;

public abstract class SharedTricorderSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TricorderComponent, ComponentGetState>(GetTricorderState);
    }

    private static void GetTricorderState(EntityUid uid, TricorderComponent component, ref ComponentGetState args)
    {
        args.State = new TricorderComponentState(component.CurrentMode);
    }

    public static string GetNameByMode(TricorderMode mode)
    {
        return mode switch
        {
            TricorderMode.Multitool      => "[color=yellow]мультитул[/color]",
            TricorderMode.GasAnalyzer    => "[color=cyan]газоанализатор[/color]",
            TricorderMode.HealthAnalyzer => "[color=green]анализатор здоровья[/color]",
            _                            => "[color=yellow]мультитул[/color]"
        };
    }
}