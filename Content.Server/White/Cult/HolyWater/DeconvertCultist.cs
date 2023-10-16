using System.Threading;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.IdentityManagement;
using Content.Shared.White.Cult;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.White.Cult.HolyWater;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public sealed class DeconvertCultist : ReagentEffect
{
    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-deconvert-cultist");
    }

    public override void Effect(ReagentEffectArgs args)
    {
        var uid = args.SolutionEntity;

        if (!args.EntityManager.TryGetComponent(uid, out CultistComponent? component))
            return;

        if (component.HolyConvertToken != null)
            return;

        args.EntityManager.System<StunSystem>()
            .TryParalyze(uid, TimeSpan.FromSeconds(component.HolyConvertTime + 5f), true);
        var target = Identity.Name(uid, args.EntityManager);
        args.EntityManager.System<PopupSystem>()
            .PopupEntity(Loc.GetString("holy-water-started-converting", ("target", target)), uid);

        component.HolyConvertToken = new CancellationTokenSource();
        Timer.Spawn(TimeSpan.FromSeconds(component.HolyConvertTime), () => ConvertCultist(uid, args.EntityManager),
            component.HolyConvertToken.Token);
    }

    private void ConvertCultist(EntityUid uid, IEntityManager entityManager)
    {
        if (!entityManager.TryGetComponent<CultistComponent>(uid, out var cultist))
            return;

        cultist.HolyConvertToken = null;
        entityManager.RemoveComponent<CultistComponent>(uid);
        entityManager.RemoveComponent<PentagramComponent>(uid);
    }
}
