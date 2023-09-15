using System.Diagnostics;
using System.Linq;
using Content.Server.Mind;
using Content.Server.Objectives.Interfaces;
using Content.Server.White.Cult.GameRule;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions;

[DataDefinition]
public sealed class KillCultistTarget : IObjectiveCondition
{
    private IEntityManager EntityManager => IoCManager.Resolve<IEntityManager>();

    private Mind.Mind? _target;

    public IObjectiveCondition GetAssigned(Mind.Mind mind)
    {
        var cultistRule = EntityManager.EntityQuery<CultRuleComponent>().FirstOrDefault();
        Debug.Assert(cultistRule != null, nameof(cultistRule) + " != null");
        var target = cultistRule.CultTarget;

        return new KillCultistTarget()
        {
            _target = target
        };
    }

    public string Title
    {
        get
        {
            var targetName = string.Empty;
            var jobName = _target?.CurrentJob?.Name ?? "Unknown";

            if (_target == null)
                return Loc.GetString("objective-condition-kill-person-title", ("targetName", targetName), ("job", jobName));

            if (_target.OwnedEntity is {Valid: true} owned)
                targetName = EntityManager.GetComponent<MetaDataComponent>(owned).EntityName;

            return Loc.GetString("objective-condition-kill-person-title", ("targetName", targetName), ("job", jobName));
        }
    }

    public string Description => Loc.GetString("objective-condition-kill-person-description");

    public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new ("Objects/Weapons/Guns/Pistols/viper.rsi"), "icon");

    public float Progress
    {
        get
        {
            var entityManager = IoCManager.Resolve<EntityManager>();
            var mindSystem = entityManager.System<MindSystem>();
            return _target == null || mindSystem.IsCharacterDeadIc(_target) ? 1f : 0f;
        }
    }

    public float Difficulty => 2f;

    public bool Equals(IObjectiveCondition? other)
    {
        return other is KillCultistTarget kpc && Equals(_target, kpc._target);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj.GetType() == GetType() && Equals((KillCultistTarget) obj);
    }

    public override int GetHashCode()
    {
        return _target?.GetHashCode() ?? 0;
    }
}
