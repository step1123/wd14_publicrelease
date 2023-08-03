using System.Linq;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.White.Other.FootPrints;
using Robust.Shared.Physics.Events;

namespace Content.Server.White.Other.FootPrints;

public sealed class PuddleFootPrintsSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;

   /* public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PuddleFootPrintsComponent, EndCollideEvent>(OnStepTrigger);
    } */

    private void OnStepTrigger(EntityUid uid, PuddleFootPrintsComponent comp, ref EndCollideEvent args)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance) ||
            !TryComp<FootPrintsComponent>(args.OtherEntity, out var tripper) ||
            !TryComp<SolutionContainerManagerComponent>(uid, out var solutionManager))
        {
            return;
        }

        if (!_solutionContainerSystem.TryGetSolution(uid, "puddle", out var solutions, solutionManager))
            return;

        var totalSolutionQuantity = solutions.Contents.Sum(sol => (float)sol.Quantity);
        var waterQuantity = (from sol in solutions.Contents where sol.ReagentId == "Water" select (float) sol.Quantity).FirstOrDefault();

        if (waterQuantity / (totalSolutionQuantity / 100f) > comp.OffPercent)
            return;

        if (_appearance.TryGetData(uid, PuddleVisuals.SolutionColor, out var color, appearance) &&
            _appearance.TryGetData(uid, PuddleVisuals.CurrentVolume, out var volume, appearance))
        {
            AddColor((Color)color, (float)volume * comp.SizeRatio, tripper);
        }
    }

    private void AddColor(Color col, float quantity, FootPrintsComponent comp)
    {
        comp.PrintsColor = comp.ColorQuantity == 0f ? col : Color.InterpolateBetween(comp.PrintsColor, col, 0.2f);
        comp.ColorQuantity += quantity;
    }
}
