using Content.Server.Mind.Components;
using Content.Shared.Store;

namespace Content.Server.Store.Conditions;

public sealed class BuyerBlockForMindProtected : ListingCondition
{
    public override bool Condition(ListingConditionArgs args)
    {
        var buyer = args.Buyer;
        var ent = args.EntityManager;

        if (!ent.TryGetComponent<MindComponent>(buyer, out var mind) || mind.Mind == null)
            return false;

        if (mind.Mind.CurrentJob?.CanBeAntag != true)
            return false;

        return true;
    }
}
