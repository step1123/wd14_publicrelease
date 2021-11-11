using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Ghost.Components;
using Content.Server.Mind.Components;
using Content.Server.Objectives;
using Content.Server.Players;
using Content.Server.Roles;
using Content.Shared.MobState.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Server.Mind
{
    /// <summary>
    ///     A mind represents the IC "mind" of a player. Stores roles currently.
    /// </summary>
    /// <remarks>
    ///     Think of it like this: if a player is supposed to have their memories,
    ///     their mind follows along.
    ///
    ///     Things such as respawning do not follow, because you're a new character.
    ///     Getting borged, cloned, turned into a catbeast, etc... will keep it following you.
    /// </remarks>
    public sealed class Mind
    {
        private readonly ISet<Role> _roles = new HashSet<Role>();

        private readonly List<Objective> _objectives = new();

        /// <summary>
        ///     Creates the new mind attached to a specific player session.
        /// </summary>
        /// <param name="userId">The session ID of the owning player.</param>
        public Mind(NetUserId userId)
        {
            UserId = userId;
        }

        // TODO: This session should be able to be changed, probably.
        /// <summary>
        ///     The session ID of the player owning this mind.
        /// </summary>
        [ViewVariables]
        public NetUserId? UserId { get; private set; }

        [ViewVariables]
        public bool IsVisitingEntity => VisitingEntity != null;

        [ViewVariables]
        public IEntity? VisitingEntity { get; private set; }

        [ViewVariables] public IEntity? CurrentEntity => VisitingEntity ?? OwnedEntity;

        [ViewVariables(VVAccess.ReadWrite)]
        public string? CharacterName { get; set; }

        /// <summary>
        ///     The time of death for this Mind.
        ///     Can be null - will be null if the Mind is not considered "dead".
        /// </summary>
        [ViewVariables] public TimeSpan? TimeOfDeath { get; set; } = null;

        /// <summary>
        ///     The component currently owned by this mind.
        ///     Can be null.
        /// </summary>
        [ViewVariables]
        public MindComponent? OwnedComponent { get; private set; }

        /// <summary>
        ///     The entity currently owned by this mind.
        ///     Can be null.
        /// </summary>
        [ViewVariables]
        public IEntity? OwnedEntity => OwnedComponent?.Owner;

        /// <summary>
        ///     An enumerable over all the roles this mind has.
        /// </summary>
        [ViewVariables]
        public IEnumerable<Role> AllRoles => _roles;

        /// <summary>
        ///     An enumerable over all the objectives this mind has.
        /// </summary>
        [ViewVariables]
        public IEnumerable<Objective> AllObjectives => _objectives;

        /// <summary>
        ///     The session of the player owning this mind.
        ///     Can be null, in which case the player is currently not logged in.
        /// </summary>
        [ViewVariables]
        public IPlayerSession? Session
        {
            get
            {
                if (!UserId.HasValue)
                {
                    return null;
                }
                var playerMgr = IoCManager.Resolve<IPlayerManager>();
                playerMgr.TryGetSessionById(UserId.Value, out var ret);
                return ret;
            }
        }

        /// <summary>
        ///     True if this Mind is 'sufficiently dead' IC (objectives, endtext).
        ///     Note that this is *IC logic*, it's not necessarily tied to any specific truth.
        ///     "If administrators decide that zombies are dead, this returns true for zombies."
        ///     (Maybe you were looking for the action blocker system?)
        /// </summary>
        [ViewVariables]
        public bool CharacterDeadIC => CharacterDeadPhysically;

        /// <summary>
        ///     True if the OwnedEntity of this mind is physically dead.
        ///     This specific definition, as opposed to CharacterDeadIC, is used to determine if ghosting should allow return.
        /// </summary>
        [ViewVariables]
        public bool CharacterDeadPhysically
        {
            get
            {
                // This is written explicitly so that the logic can be understood.
                // But it's also weird and potentially situational.
                // Specific considerations when updating this:
                //  + Does being turned into a borg (if/when implemented) count as dead?
                //    *If not, add specific conditions to users of this property where applicable.*
                //  + Is being transformed into a donut 'dead'?
                //    TODO: Consider changing the way ghost roles work.
                //    Mind is an *IC* mind, therefore ghost takeover is IC revival right now.
                //  + Is it necessary to have a reference to a specific 'mind iteration' to cycle when certain events happen?
                //    (If being a borg or AI counts as dead, then this is highly likely, as it's still the same Mind for practical purposes.)

                // This can be null if they're deleted (spike / brain nom)
                if (OwnedEntity == null)
                    return true;
                var targetMobState = OwnedEntity.GetComponentOrNull<MobStateComponent>();
                // This can be null if it's a brain (this happens very often)
                // Brains are the result of gibbing so should definitely count as dead
                if (targetMobState == null)
                    return true;
                // They might actually be alive.
                return targetMobState.IsDead();
            }
        }

        /// <summary>
        ///     Gives this mind a new role.
        /// </summary>
        /// <param name="role">The type of the role to give.</param>
        /// <returns>The instance of the role.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if we already have a role with this type.
        /// </exception>
        public Role AddRole(Role role)
        {
            if (_roles.Contains(role))
            {
                throw new ArgumentException($"We already have this role: {role}");
            }

            _roles.Add(role);
            role.Greet();

            var message = new RoleAddedEvent(role);
            OwnedEntity?.EntityManager.EventBus.RaiseLocalEvent(OwnedEntity.Uid, message);

            return role;
        }

        /// <summary>
        ///     Removes a role from this mind.
        /// </summary>
        /// <param name="role">The type of the role to remove.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown if we do not have this role.
        /// </exception>
        public void RemoveRole(Role role)
        {
            if (!_roles.Contains(role))
            {
                throw new ArgumentException($"We do not have this role: {role}");
            }

            _roles.Remove(role);

            var message = new RoleRemovedEvent(role);
            OwnedEntity?.EntityManager.EventBus.RaiseLocalEvent(OwnedEntity.Uid, message);
        }

        public bool HasRole<T>() where T : Role
        {
            var t = typeof(T);

            return _roles.Any(role => role.GetType() == t);
        }

        /// <summary>
        /// Adds an objective to this mind.
        /// </summary>
        public bool TryAddObjective(ObjectivePrototype objectivePrototype)
        {
            if (!objectivePrototype.CanBeAssigned(this))
                return false;
            var objective = objectivePrototype.GetObjective(this);
            if (_objectives.Contains(objective))
                return false;
            _objectives.Add(objective);
            return true;
        }

        /// <summary>
        /// Removes an objective to this mind.
        /// </summary>
        /// <returns>Returns true if the removal succeeded.</returns>
        public bool TryRemoveObjective(int index)
        {
            if (_objectives.Count >= index) return false;

            var objective = _objectives[index];
            _objectives.Remove(objective);
            return true;
        }



        /// <summary>
        ///     Transfer this mind's control over to a new entity.
        /// </summary>
        /// <param name="entity">
        ///     The entity to control.
        ///     Can be null, in which case it will simply detach the mind from any entity.
        /// </param>
        /// <param name="ghostCheckOverride">
        ///     If true, skips ghost check for Visiting Entity
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown if <paramref name="entity"/> is already owned by another mind.
        /// </exception>
        public void TransferTo(IEntity? entity, bool ghostCheckOverride = false)
        {
            MindComponent? component = null;
            var alreadyAttached = false;

            if (entity != null)
            {
                if (!entity.TryGetComponent(out component))
                {
                    component = entity.AddComponent<MindComponent>();
                }
                else if (component.HasMind)
                {
                    EntitySystem.Get<GameTicker>().OnGhostAttempt(component.Mind!, false);
                }

                if (entity.TryGetComponent(out ActorComponent? actor))
                {
                    // Happens when transferring to your currently visited entity.
                    if (actor.PlayerSession != Session)
                    {
                        throw new ArgumentException("Visit target already has a session.", nameof(entity));
                    }

                    alreadyAttached = true;
                }
            }

            OwnedComponent?.InternalEjectMind();

            OwnedComponent = component;
            OwnedComponent?.InternalAssignMind(this);

            if (IsVisitingEntity
                && (ghostCheckOverride // to force mind transfer, for example from ControlMobVerb
                || !VisitingEntity!.TryGetComponent(out GhostComponent? ghostComponent) // visiting entity is not a Ghost
                || !ghostComponent.CanReturnToBody))  // it is a ghost, but cannot return to body anyway, so it's okay
            {
                VisitingEntity = null;
            }

            // Player is CURRENTLY connected.
            if (Session != null && !alreadyAttached && VisitingEntity == null)
            {
                Session.AttachToEntity(entity);
                Logger.Info($"Session {Session.Name} transferred to entity {entity}.");
            }
        }

        public void RemoveOwningPlayer()
        {
            UserId = null;
        }

        public void ChangeOwningPlayer(NetUserId? newOwner)
        {
            var playerMgr = IoCManager.Resolve<IPlayerManager>();
            PlayerData? newOwnerData = null;

            if (newOwner.HasValue)
            {
                if (!playerMgr.TryGetPlayerData(newOwner.Value, out var uncast))
                {
                    // This restriction is because I'm too lazy to initialize the player data
                    // for a client that hasn't logged in yet.
                    // Go ahead and remove it if you need.
                    throw new ArgumentException("new owner must have previously logged into the server.");
                }

                newOwnerData = uncast.ContentData();
            }

            // Make sure to remove control from our old owner if they're logged in.
            var oldSession = Session;
            oldSession?.AttachToEntity(null);

            if (UserId.HasValue)
            {
                var data = playerMgr.GetPlayerData(UserId.Value).ContentData();
                DebugTools.AssertNotNull(data);
                data!.Mind = null;
            }

            UserId = newOwner;
            if (!newOwner.HasValue)
            {
                return;
            }

            // Yank new owner out of their old mind too.
            // Can I mention how much I love the word yank?
            DebugTools.AssertNotNull(newOwnerData);
            newOwnerData!.Mind?.ChangeOwningPlayer(null);
            newOwnerData.Mind = this;
        }

        public void Visit(IEntity entity)
        {
            Session?.AttachToEntity(entity);
            VisitingEntity = entity;

            var comp = entity.AddComponent<VisitingMindComponent>();
            comp.Mind = this;

            Logger.Info($"Session {Session?.Name} visiting entity {entity}.");
        }

        public void UnVisit()
        {
            if (!IsVisitingEntity)
            {
                return;
            }

            Session?.AttachToEntity(OwnedEntity);
            var oldVisitingEnt = VisitingEntity;
            // Null this before removing the component to avoid any infinite loops.
            VisitingEntity = null;

            DebugTools.AssertNotNull(oldVisitingEnt);

            if (oldVisitingEnt!.HasComponent<VisitingMindComponent>())
            {
                oldVisitingEnt.RemoveComponent<VisitingMindComponent>();
            }

            oldVisitingEnt.EntityManager.EventBus.RaiseLocalEvent(oldVisitingEnt.Uid, new MindUnvisitedMessage());
        }

        public bool TryGetSession([NotNullWhen(true)] out IPlayerSession? session)
        {
            return (session = Session) != null;
        }
    }
}
