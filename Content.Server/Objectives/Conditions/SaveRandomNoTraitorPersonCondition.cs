using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.Objectives.Interfaces;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;


namespace Content.Server.Objectives.Conditions
{
    [DataDefinition]
    public sealed class SaveRandomNoTraitorPersonCondition : IObjectiveCondition
    {
        private IEntityManager EntityManager => IoCManager.Resolve<IEntityManager>();
        private MobStateSystem MobStateSystem => EntityManager.EntitySysManager.GetEntitySystem<MobStateSystem>();
        private MindSystem MindSystem => EntityManager.System<MindSystem>();

        private Mind.Mind? _target;

        public string Description => Loc.GetString("objective-condition-save-no-traitor-person-description");

        public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new("Objects/Misc/bureaucracy.rsi"), "folder-white");

        public float Difficulty => 1.2f;

        public string Title
        {
            get
            {
                var targetName = string.Empty;
                var jobName = _target?.CurrentJob?.Name ?? "Unknown";

                if (_target == null)
                    return Loc.GetString("objective-condition-save-no-traitor-person-title", ("targetName", targetName), ("job", jobName));

                if (_target.OwnedEntity is { Valid: true } owned)
                    targetName = IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(owned).EntityName;

                return Loc.GetString("objective-condition-save-no-traitor-person-title", ("targetName", targetName), ("job", jobName));
            }
        }

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            var allPerson = EntityManager.EntityQuery<MindContainerComponent>(true).Where(mindComponent =>
            {
                var entity = mindComponent.Mind?.OwnedEntity;

                if (entity == default)
                    return false;

                var rolesAntag = mindComponent.Mind?.AllRoles.Where(role => role.Antagonist).ToList();

                return EntityManager.TryGetComponent(entity, out MobStateComponent? mobState) &&
                      MobStateSystem.IsAlive(entity.Value, mobState) &&
                       mindComponent.Mind != mind && mindComponent?.Mind?.Session is not null && rolesAntag?.Count == 0;

            }).Select(mc => mc.Mind).ToList();

            if (allPerson.Count == 0)
                return new DieCondition();

            return new SaveRandomNoTraitorPersonCondition { _target = IoCManager.Resolve<IRobustRandom>().Pick(allPerson) };
        }

        public float Progress
        {
            get
            {
                return _target == null || MindSystem.IsCharacterDeadIc(_target) ? 0f : 1f;
            }
        }

        public bool Equals(IObjectiveCondition? other)
        {
            return other is SaveRandomNoTraitorPersonCondition kpc && Equals(_target, kpc._target);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SaveRandomNoTraitorPersonCondition alive && alive.Equals(this);
        }

        public override int GetHashCode()
        {
            return _target?.GetHashCode() ?? 0;
        }
    }

}
