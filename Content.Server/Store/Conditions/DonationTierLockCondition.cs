using Content.Server.White.Sponsors;
using Content.Shared.Store;
using Robust.Server.GameObjects;

namespace Content.Server.Store.Conditions;

public sealed class DonationTierLockCondition : ListingCondition
{
    [DataField("tier", required: true)]
    public int Tier;
    public override bool Condition(ListingConditionArgs args)
    {
        var entityManager = args.EntityManager;
        var sponsorsManager = IoCManager.Resolve<SponsorsManager>();

        if(!entityManager.TryGetComponent<ActorComponent>(args.Buyer, out var actor)) return false;

        if(!sponsorsManager.TryGetInfo(actor.PlayerSession.UserId, out var sponsorInfo)) return false;

        if (sponsorInfo.Tier < Tier) return false;

        return true;
    }
}
