using Content.Server.Mind;
using Content.Server.Objectives.Interfaces;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed class StayAliveCondition : IObjectiveCondition
    {
        private IEntityManager EntityManager => IoCManager.Resolve<IEntityManager>();

        private Mind.Mind? _mind;

        public string Title => Loc.GetString("objective-condition-stay-alive-title");

        public string Description => Loc.GetString("objective-condition-stay-alive-description");

        public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new("Mobs/Species/Human/parts.rsi"), "full");

        public float Difficulty => 0.8f;

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            return new StayAliveCondition { _mind = mind };
        }

        public float Progress
        {
            get
            {
                var mindSystem = EntityManager.System<MindSystem>();
                return _mind == null || !mindSystem.IsCharacterDeadIc(_mind) ? 1f : 0f;
            }
        }

        public bool Equals(IObjectiveCondition? other)
        {
            return other is StayAliveCondition condition && Equals(_mind, condition._mind);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StayAliveCondition) obj);
        }

        public override int GetHashCode()
        {
            return (_mind != null ? _mind.GetHashCode() : 0);
        }
    }
}
