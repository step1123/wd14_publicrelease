using Content.Server.Objectives.Interfaces;

namespace Content.Server.Objectives.Conditions;

[DataDefinition]
public sealed class KillAllSecurityCondition : KillDepartmentCondition
{
    protected override string TitleHeader => Loc.GetString("objective-condition-all-security-title");
    public override IObjectiveCondition GetAssigned(Mind.Mind mind)
    {
        var allMinds = GetTargets(mind, "Security");

        if (allMinds.Count == 0)
        {
            var condition = new DieCondition();
            return condition.GetAssigned(mind);
        }

        return new KillAllSecurityCondition()
        {
            Targets = allMinds
        };
    }
}
