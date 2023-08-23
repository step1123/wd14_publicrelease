using System.Linq;
using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.Objectives.Interfaces;
using Content.Server.Roles;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions;

public abstract class KillDepartmentCondition : IObjectiveCondition
{
    private IEntityManager EntityManager => IoCManager.Resolve<IEntityManager>();
    private IPrototypeManager PrototypeManager => IoCManager.Resolve<IPrototypeManager>();
    private MobStateSystem MobStateSystem => EntityManager.EntitySysManager.GetEntitySystem<MobStateSystem>();
    private MindSystem MindSystem => EntityManager.EntitySysManager.GetEntitySystem<MindSystem>();

    protected abstract string TitleHeader { get; }
    public abstract IObjectiveCondition GetAssigned(Mind.Mind mind);

    protected List<Mind.Mind?>? Targets;

    protected List<Mind.Mind?> GetTargets(Mind.Mind mind, string department)
    {
        var dep = PrototypeManager.Index<DepartmentPrototype>(department);
        var allMinds = EntityManager.EntityQuery<MindContainerComponent>(true).Where(mc =>
        {
            var entity = mc.Mind?.OwnedEntity;

            if (entity == default || mc.Mind == mind)
                return false;

            var isTargetJob = mc.Mind?.AllRoles.OfType<Job>().Any(job => dep.Roles.Contains(job.Prototype.ID));

            if (isTargetJob is false)
                return false;

            return EntityManager.TryGetComponent(entity, out MobStateComponent? mobState) &&
                  MobStateSystem.IsAlive(entity.Value, mobState);

        }).Select(mc => mc.Mind).ToList();

        return allMinds;
    }

    public string Title
    {
        get
        {
            var title = TitleHeader;
            if (Targets == null)
                return title;

            foreach (var target in Targets)
            {
                if (target?.OwnedEntity is { Valid: true } owned)
                {
                    title += $"\n  - {EntityManager.GetComponent<MetaDataComponent>(owned).EntityName}, " +
                             $"{target?.CurrentJob?.Name ?? "Unknown"}";
                }
            }

            return title;
        }
    }

    public string Description => Loc.GetString("objective-condition-department-description");

    public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new ("Objects/Weapons/Guns/Pistols/viper.rsi"), "icon");

    public float Progress
    {
        get
        {
            if (Targets == null)
                return 1f;

            var deadTargetsCount = Targets.Count(target => target != null && MindSystem.IsCharacterDeadIc(target));

            return Math.Min(1f / Targets.Count * deadTargetsCount, 1f);
        }
    }

    public float Difficulty => 4f;
    public bool Equals(IObjectiveCondition? other)
    {
        return other is KillDepartmentCondition kdc && Equals(Targets, kdc.Targets);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj.GetType() == GetType() && Equals((KillDepartmentCondition) obj);
    }

    public override int GetHashCode()
    {
        return Targets?.GetHashCode() ?? 0;
    }
}
