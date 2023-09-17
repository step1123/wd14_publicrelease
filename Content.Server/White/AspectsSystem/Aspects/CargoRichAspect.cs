using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class CargoRichAspect : AspectSystem<CargoRichAspectComponent>
{
    [Dependency] private readonly CargoSystem _cargoSystem = default!;

    protected override void Added(EntityUid uid, CargoRichAspectComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station))
            return;

        if (!TryComp<StationBankAccountComponent>(station, out var stationBankAccountComponent))
            return;

        _cargoSystem.UpdateBankAccount(station, stationBankAccountComponent, 100000);
    }
}
