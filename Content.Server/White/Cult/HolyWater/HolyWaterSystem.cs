using System.Linq;
using System.Threading;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.White.Cult;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.White.Cult.HolyWater;

public sealed class HolyWaterSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultistComponent, SolutionChangedEvent>(OnSolutionChanged);
        SubscribeLocalEvent<BibleWaterConvertComponent, AfterInteractEvent>(OnBibleInteract);
    }

    private void OnBibleInteract(EntityUid uid, BibleWaterConvertComponent component, AfterInteractEvent args)
    {
        if (HasComp<MobStateComponent>(uid))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(args.Target, out var container))
            return;

        foreach (var solution in container.Solutions.Values.Where(solution => solution.ContainsReagent(component.ConvertedId)))
        {
            foreach (var reagent in solution.Contents)
            {
                if (reagent.ReagentId != component.ConvertedId)
                    continue;

                var amount = reagent.Quantity;

                solution.RemoveReagent(reagent.ReagentId, reagent.Quantity);
                solution.AddReagent(component.ConvertedToId, amount);
                return;
            }
        }
    }

    private void OnSolutionChanged(EntityUid uid, CultistComponent component, SolutionChangedEvent args)
    {
        if (!args.Solution.ContainsReagent("HolyWater"))
            return;

        if (component.HolyConvertToken != null)
            return;

        _stun.TryParalyze(uid, TimeSpan.FromSeconds(component.HolyConvertTime + 5f), true);

        component.HolyConvertToken = new CancellationTokenSource();
        Timer.Spawn(TimeSpan.FromSeconds(component.HolyConvertTime), () => ConvertCultist(uid), component.HolyConvertToken.Token);
    }

    private void ConvertCultist(EntityUid uid)
    {
        if (!TryComp<CultistComponent>(uid, out var cultist))
            return;

        cultist.HolyConvertToken = null;
        RemComp<CultistComponent>(uid);
        RemComp<PentagramComponent>(uid);
    }
}
