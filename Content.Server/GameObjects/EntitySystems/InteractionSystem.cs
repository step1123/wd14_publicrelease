﻿using Content.Server.Interfaces.GameObjects;
using SS14.Server.Interfaces.GameObjects;
using SS14.Shared.GameObjects;
using SS14.Shared.GameObjects.System;
using SS14.Shared.Interfaces.GameObjects;
using System.Collections.Generic;
using System.Linq;
using SS14.Shared.Input;
using SS14.Shared.Interfaces.Network;
using SS14.Shared.IoC;
using SS14.Server.Interfaces.Player;
using SS14.Shared.Log;
using SS14.Shared.Map;
using SS14.Server.GameObjects;

namespace Content.Server.GameObjects.EntitySystems
{
    /// <summary>
    /// This interface gives components behavior when being clicked on or "attacked" by a user with an object in their hand
    /// </summary>
    public interface IAttackby
    {
        /// <summary>
        /// Called when using one object on another
        /// </summary>
        /// <param name="user"></param>
        /// <param name="attackwith"></param>
        /// <returns></returns>
        bool Attackby(IEntity user, IEntity attackwith);
    }

    /// <summary>
    /// This interface gives components behavior when being clicked on or "attacked" by a user with an empty hand
    /// </summary>
    public interface IAttackHand
    {
        /// <summary>
        /// Called when a player directly interacts with an empty hand
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool Attackhand(IEntity user);
    }

    /// <summary>
    /// This interface gives components behavior when being clicked by objects outside the range of direct use
    /// </summary>
    public interface IRangedAttackby
    {
        /// <summary>
        /// Called when we try to interact with an entity out of range
        /// </summary>
        /// <param name="user"></param>
        /// <param name="attackwith"></param>
        /// <param name="clicklocation"></param>
        /// <returns></returns>
        bool RangedAttackby(IEntity user, IEntity attackwith, LocalCoordinates clicklocation);
    }

    /// <summary>
    /// This interface gives components a behavior when clicking on another object and no interaction occurs
    /// Doesn't pass what you clicked on as an argument, but if it becomes necessary we can add it later
    /// </summary>
    public interface IAfterAttack
    {
        /// <summary>
        /// Called when we interact with nothing, or when we interact with an entity out of range that has no behavior
        /// </summary>
        /// <param name="user"></param>
        /// <param name="clicklocation"></param>
        void Afterattack(IEntity user, LocalCoordinates clicklocation);
    }

    /// <summary>
    /// This interface gives components behavior when using the entity in your hands
    /// </summary>
    public interface IUse
    {
        /// <summary>
        /// Called when we activate an object we are holding to use it
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool UseEntity(IEntity user);
    }

    /// <summary>
    /// Governs interactions during clicking on entities
    /// </summary>
    public class InteractionSystem : EntitySystem
    {
        private const float INTERACTION_RANGE = 2;
        private const float INTERACTION_RANGE_SQUARED = INTERACTION_RANGE * INTERACTION_RANGE;

        /// <inheritdoc />
        public override void RegisterMessageTypes()
        {
            base.RegisterMessageTypes();

            RegisterMessageType<ClickEventMessage>();
        }

        //Grab click events sent from the client input system
        public override void HandleNetMessage(INetChannel channel, EntitySystemMessage message)
        {
            base.HandleNetMessage(channel, message);

            var playerMan = IoCManager.Resolve<IPlayerManager>();
            var session = playerMan.GetSessionByChannel(channel);
            var playerentity = session.AttachedEntity;

            if (playerentity == null)
                return;

            switch (message)
            {
                case ClickEventMessage msg:
                    UserInteraction(msg, playerentity);
                    break;
            }
        }

        private void UserInteraction(ClickEventMessage msg, IEntity player)
        {
            //Verify click type
            if (msg.Click != ClickType.Left)
                return;

            //Get entity clicked upon from UID if valid UID, if not assume no entity clicked upon and null
            IEntity attacked = null;
            if (msg.Uid.IsValid())
                attacked = EntityManager.GetEntity(msg.Uid);
            
            //Verify player has a transform component
            if (!player.TryGetComponent<IServerTransformComponent>(out var playerTransform))
            {
                return;
            }
            //Verify player is on the same map as the entity he clicked on
            else if (msg.Coordinates.MapID != playerTransform.MapID)
            {
                Logger.Warning(string.Format("Player named {0} clicked on a map he isn't located on", player.Name));
                return;
            }

            //Verify player has a hand, and find what object he is currently holding in his active hand
            if (!player.TryGetComponent<IHandsComponent>(out var hands))
            {
                return;
            }
            var item = hands.GetHand(hands.ActiveIndex)?.Owner;


            //TODO: Mob status code that allows or rejects interactions based on current mob status
            //Check if client should be able to see that object to click on it in the first place, prevent using locaters by firing a laser or something

            //Off entity click handling
            if (attacked == null)
            {
                if(item != null)
                {
                    //AFTERATTACK: Check if we clicked on an empty location, if so the only interaction we can do is afterattack
                    InteractAfterattack(player, item, msg.Coordinates);
                    return;
                }
                return;
            }
            //USE: Check if we clicked on the item we are holding in our active hand to use it
            else if(attacked == item && item != null)
            {
                UseInteraction(player, item);
                return;
            }

            //Check if ClickLocation is in object bounds here, if not lets log as warning and see why
            if(attacked != null && attacked.TryGetComponent(out BoundingBoxComponent boundingbox))
            {
                if (!boundingbox.WorldAABB.Contains(msg.Coordinates.Position))
                {
                    Logger.Warning(string.Format("Player {0} clicked {1} outside of its bounding box component somehow", player.Name, attacked.Name));
                    return;
                }
            }

            //RANGEDATTACK/AFTERATTACK: Check distance between user and clicked item, if too large parse it in the ranged function
            //TODO: have range based upon the item being used? or base it upon some variables of the player himself?
            var distance = (playerTransform.WorldPosition - attacked.GetComponent<IServerTransformComponent>().WorldPosition).LengthSquared;
            if (distance > INTERACTION_RANGE_SQUARED)
            {
                if(item != null)
                {
                    RangedInteraction(player, item, attacked, msg.Coordinates);
                    return;
                }
                return; //Add some form of ranged attackhand here if you need it someday, or perhaps just ways to modify the range of attackhand
            }
            
            //We are close to the nearby object and the object isn't contained in our active hand
            //ATTACKBY/AFTERATTACK: We will either use the item on the nearby object
            if (item != null)
            {
                Interaction(player, item, attacked, msg.Coordinates);
            }
            //ATTACKHAND: Since our hand is empty we will use attackhand
            else
            {
                Interaction(player, attacked);
            }
        }

        /// <summary>
        /// We didn't click on any entity, try doing an afterattack on the click location
        /// </summary>
        /// <param name="user"></param>
        /// <param name="weapon"></param>
        /// <param name="clicklocation"></param>
        private void InteractAfterattack(IEntity user, IEntity weapon, LocalCoordinates clicklocation)
        {
            //If not lets attempt to use afterattack from the held item on the click location
            if (weapon.TryGetComponent(out IAfterAttack attacker))
            {
                attacker.Afterattack(user, clicklocation);
            }
        }

        /// <summary>
        /// Uses a weapon/object on an entity
        /// Finds interactable components with the Attackby interface and calls their function
        /// </summary>
        /// <param name="user"></param>
        /// <param name="weapon"></param>
        /// <param name="attacked"></param>
        public static void Interaction(IEntity user, IEntity weapon, IEntity attacked, LocalCoordinates clicklocation)
        {
            List<IAttackby> interactables = attacked.GetComponents<IAttackby>().ToList();

            for(var i = 0; i < interactables.Count; i++)
            {
                if (interactables[i].Attackby(user, weapon)) //If an attackby returns a status completion we finish our attack
                {
                    return;
                }
            }

            //Else check damage component to see if we damage if not attackby, and if so can we attack object

            //If we aren't directly attacking the nearby object, lets see if our item has an after attack we can do
            if (weapon.TryGetComponent(out IAfterAttack attacker))
            {
                attacker.Afterattack(user, clicklocation);
            }
        }

        /// <summary>
        /// Uses an empty hand on an entity
        /// Finds interactable components with the Attackhand interface and calls their function
        /// </summary>
        /// <param name="user"></param>
        /// <param name="attacked"></param>
        public static void Interaction(IEntity user, IEntity attacked)
        {
            List<IAttackHand> interactables = attacked.GetComponents<IAttackHand>().ToList();

            for (var i = 0; i < interactables.Count; i++)
            {
                if (interactables[i].Attackhand(user)) //If an attackby returns a status completion we finish our attack
                {
                    return;
                }
            }

            //Else check damage component to see if we damage if not attackby, and if so can we attack object
        }

        /// <summary>
        /// Activates/Uses an object in control/possession of a user
        /// If the item has the IUse interface on one of its components we use the object in our hand
        /// </summary>
        /// <param name="user"></param>
        /// <param name="attacked"></param>
        public static void UseInteraction(IEntity user, IEntity used)
        {
            List<IUse> usables = used.GetComponents<IUse>().ToList();

            //Try to use item on any components which have the interface
            for (var i = 0; i < usables.Count; i++)
            {
                if (usables[i].UseEntity(user)) //If an attackby returns a status completion we finish our attack
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Will have two behaviors, either "uses" the weapon at range on the entity if it is capable of accepting that action
        /// Or it will use the weapon itself on the position clicked, regardless of what was there
        /// </summary>
        /// <param name="user"></param>
        /// <param name="weapon"></param>
        /// <param name="attacked"></param>
        public static void RangedInteraction(IEntity user, IEntity weapon, IEntity attacked, LocalCoordinates clicklocation)
        {
            List<IRangedAttackby> rangedusables = attacked.GetComponents<IRangedAttackby>().ToList();

            //See if we have a ranged attack interaction
            for (var i = 0; i < rangedusables.Count; i++)
            {
                if (rangedusables[i].RangedAttackby(user, weapon, clicklocation)) //If an attackby returns a status completion we finish our attack
                {
                    return;
                }
            }

            //If not lets attempt to use afterattack from the held item on the click location
            if (weapon != null && weapon.TryGetComponent(out IAfterAttack attacker))
            {
                attacker.Afterattack(user, clicklocation);
            }
        }
    }
}
