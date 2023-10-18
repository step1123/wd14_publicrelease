using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Server.Traitor.Uplink;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.FixedPoint;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class TraitorRichAspect : AspectSystem<TraitorRichAspectComponent>
{
    [Dependency] private readonly TraitorRuleSystem _traitorRuleSystem = default!;
    [Dependency] private readonly UplinkSystem _uplinkSystem = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private const string BriefingExtra =
        "Поздравляем! Было принято решение выделить для вас 10 дополнительных телекристаллов.";

    protected override void Started(EntityUid uid, TraitorRichAspectComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!HasTraitorGameRule())
            ForceEndSelf(uid, gameRule);

        RewardTraitors();
    }

    private void RewardTraitors()
    {
        var traitors = _traitorRuleSystem.GetAllLivingConnectedTraitors();

        foreach (var traitor in traitors)
        {
            var ent = traitor.Mind.CurrentEntity;

            if (ent == null)
                continue;

            var uplink = _uplinkSystem.FindUplinkTarget(ent.Value);

            if (uplink == null || !TryComp(uplink, out StoreComponent? store) || store.AccountOwner != ent ||
                store.Preset != "StorePresetUplink")
                continue;

            if (_store.TryAddCurrency(
                    new Dictionary<string, FixedPoint2> {{UplinkSystem.TelecrystalCurrencyPrototype, 10}}, uplink.Value,
                    store))
            {
                NotifyTraitor(traitor.Mind, _chatManager);
            }
        }
    }

    public static void NotifyTraitor(Mind.Mind mind, IChatManager chatManager)
    {
        if (mind.Session == null)
            return;

        chatManager.DispatchServerMessage(mind.Session, BriefingExtra);
    }

    private bool HasTraitorGameRule()
    {
        return EntityQuery<TraitorRuleComponent>().Any();
    }
}
