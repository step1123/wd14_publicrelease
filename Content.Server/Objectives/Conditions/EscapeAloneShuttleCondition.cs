
using Content.Server.Objectives.Interfaces;
using JetBrains.Annotations;
using Content.Server.Shuttles.Components;
using Content.Shared.Cuffs.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using Robust.Server.Player;
using Content.Server.Players;
using Content.Shared.Body.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Humanoid;
using Content.Server.Afk;
using Robust.Shared.Player;
using Content.Server.Mind;

namespace Content.Server.Objectives.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed class EscapeAloneShuttleCondition : IObjectiveCondition
    {
        private IEntityManager EntityManager => IoCManager.Resolve<IEntityManager>();
        private MobStateSystem MobStateSystem => EntityManager.EntitySysManager.GetEntitySystem<MobStateSystem>();
        private MindSystem Mindsys => EntityManager.EntitySysManager.GetEntitySystem<MindSystem>();

        private Mind.Mind? _mind;

        public string Title => Loc.GetString("objective-condition-escape-alone-shuttle-title");

        public string Description => Loc.GetString("objective-condition-escape-alone-shuttle-description");

        public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new("Structures/Furniture/chairs.rsi"), "shuttle");

        public float Difficulty => 1.3f;

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            return new EscapeAloneShuttleCondition
            {
                _mind = mind,
            };
        }

        private bool IsAgentAloneOnShuttle(TransformComponent agentXform, EntityUid? shuttle)
        {
            if (shuttle == null)
                return false;


            if (!EntityManager.TryGetComponent<MapGridComponent>(shuttle, out var shuttleGrid) ||
                !EntityManager.TryGetComponent<TransformComponent>(shuttle, out var shuttleXform))
            {
                return false;
            }

            var shuttleBoxMatrx = shuttleXform.WorldMatrix.TransformBox(shuttleGrid.LocalAABB);
            if (!shuttleBoxMatrx.Contains(agentXform.WorldPosition))
                return false;

            foreach (var session in Filter.GetAllPlayers())
            {
                if (!EntityManager.TryGetComponent<TransformComponent>(session.AttachedEntity, out var xform))
                    break;

                if (!MobStateSystem.IsDead(session.AttachedEntity.Value) &&
                    shuttleBoxMatrx.Contains(xform.WorldPosition) &&
                    !xform.Equals(agentXform) && // not agent
                    EntityManager.HasComponent<HumanoidAppearanceComponent>(session.AttachedEntity)) // is humanoid
                {
                    return false;
                }
            };

            return true;
        }

        public float Progress
        {
            get
            {
                var entityManager = IoCManager.Resolve<IEntityManager>();

                if (_mind?.OwnedEntity == null
                    || !entityManager.TryGetComponent<TransformComponent>(_mind.OwnedEntity, out var xform))
                    return 0f;

                var shuttleContainsOnlyAgent = false;
                var agentIsAlive = !Mindsys.IsCharacterDeadIc(_mind);
                var agentIsEscaping = true;

                if (entityManager.TryGetComponent<CuffableComponent>(_mind.OwnedEntity, out var cuffed)
                    && cuffed.CuffedHandCount > 0)
                    agentIsEscaping = false;

                foreach (var stationData in entityManager.EntityQuery<StationEmergencyShuttleComponent>())
                {
                    if (IsAgentAloneOnShuttle(xform, stationData.EmergencyShuttle))
                    {
                        shuttleContainsOnlyAgent = true;
                        break;
                    }
                }

                return (shuttleContainsOnlyAgent && agentIsAlive && agentIsEscaping) ? 1f : 0f;
            }
        }

        public bool Equals(IObjectiveCondition? other)
        {
            return other is EscapeAloneShuttleCondition esc && Equals(_mind, esc._mind);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EscapeAloneShuttleCondition) obj);
        }

        public override int GetHashCode()
        {
            return _mind != null ? _mind.GetHashCode() : 0;
        }

    }

}
