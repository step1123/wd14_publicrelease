using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.Objectives.Interfaces;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.Objectives.Conditions
{
    [DataDefinition]
    public sealed class KillAllOtherAntagonistsCondition : IObjectiveCondition
    {
        private IEntityManager EntityManager => IoCManager.Resolve<IEntityManager>();
        private MobStateSystem MobStateSystem => EntityManager.EntitySysManager.GetEntitySystem<MobStateSystem>();
        private MindSystem Mindsys => EntityManager.EntitySysManager.GetEntitySystem<MindSystem>();

        public string Title
        {
            get
            {
                string title = Loc.GetString("objective-condition-kill-all-other-antagonists-title");
                if (_targets != null)
                {
                    foreach (var target in _targets)
                    {
                        if (target?.OwnedEntity is { Valid: true } owned)
                        {
                            title += ($"\n  - {EntityManager.GetComponent<MetaDataComponent>(owned).EntityName}, " +
                                $"{target?.CurrentJob?.Name ?? "Unknown"}");
                        }
                    }
                }

                return title;
            }
        }

        public string Description => Loc.GetString("objective-condition-kill-other-antagonists-description");

        public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new("Objects/Weapons/Guns/Pistols/viper.rsi"), "icon");

        private List<Mind.Mind?>? _targets;

        public float Difficulty => 5f;

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            var allMinds = EntityManager.EntityQuery<MindContainerComponent>(true).Where(mc =>
            {
                var entity = mc.Mind?.OwnedEntity;

                if (entity == default || mc.Mind == mind)
                    return false;

                var rolesAntag = mc.Mind?.AllRoles.Where(role => role.Antagonist).ToList();

                if (rolesAntag?.Count == 0)
                    return false;

                return EntityManager.TryGetComponent(entity, out MobStateComponent? mobState) &&
                      MobStateSystem.IsAlive(entity.Value, mobState);

            }).Select(mc => mc.Mind).ToList();

            if (!allMinds.Any())
            {
                var condition = new KillRandomPersonCondition();
                return condition.GetAssigned(mind);
            }

            return new KillAllOtherAntagonistsCondition
            {
                _targets = allMinds,
            };
        }

        public float Progress
        {
            get
            {
                int deadTargetsCount = 0;

                if (_targets != null)
                {
                    foreach (var target in _targets)
                    {
                        if (target != null && Mindsys.IsCharacterDeadIc(target))
                        {
                            deadTargetsCount += 1;
                        }
                    }

                    return Math.Min(1f / _targets.Count * deadTargetsCount, 1f);
                }

                return 1f;
            }
        }

        public bool Equals(IObjectiveCondition? other)
        {
            return other is KillAllOtherAntagonistsCondition kpc && Equals(_targets, kpc._targets);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KillPersonCondition) obj);
        }

        public override int GetHashCode()
        {
            return _targets?.GetHashCode() ?? 0;
        }
    }
}
