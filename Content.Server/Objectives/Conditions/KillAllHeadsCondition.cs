using System.Linq;
using Content.Server.Objectives.Interfaces;

namespace Content.Server.Objectives.Conditions;

[DataDefinition]
public sealed class KillAllHeadsCondition : KillDepartmentCondition
{
    protected override string TitleHeader => Loc.GetString("objective-condition-all-heads-title");
    public override IObjectiveCondition GetAssigned(Mind.Mind mind)
    {
        var allMinds = GetTargets(mind, "Command");

        if (allMinds.Count == 0)
        {
            var condition = new DieCondition();
            return condition.GetAssigned(mind);
        }

        return new KillAllHeadsCondition
        {
            Targets = allMinds
        };
    }
}
