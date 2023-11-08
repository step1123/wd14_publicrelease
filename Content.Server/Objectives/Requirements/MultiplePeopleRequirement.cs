using System.Linq;
using Content.Server.Mind.Components;
using Content.Server.Objectives.Interfaces;

namespace Content.Server.Objectives.Requirements;

[DataDefinition]
public sealed class MultiplePeopleRequirement : IObjectiveRequirement
{
    [DataField("minPeople")]
    private readonly int _minPeople = 50;

    public bool CanBeAssigned(Mind.Mind mind)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        return entityManager.EntityQuery<MindContainerComponent>(true)
            .Count(mc => mc.Mind?.OwnedEntity != null) >= _minPeople;
    }
}
