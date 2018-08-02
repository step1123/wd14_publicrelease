﻿using System;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.Utility;
using YamlDotNet.RepresentationModel;
using SS14.Server.GameObjects;
using Content.Server.GameObjects.EntitySystems;
using SS14.Shared.Serialization;

namespace Content.Server.GameObjects.Components.Interactable.Tools
{
    /// <summary>
    /// Tool used to weld metal together, light things on fire, or melt into constituent parts
    /// </summary>
    class WelderComponent : ToolComponent, EntitySystems.IUse, EntitySystems.IExamine
    {
        SpriteComponent spriteComponent;

        public override string Name => "Welder";

        /// <summary>
        /// Maximum fuel capacity the welder can hold
        /// </summary>
        public float FuelCapacity
        {
            get => _fuelCapacity;
            set => _fuelCapacity = value;
        }
        private float _fuelCapacity = 50;

        /// <summary>
        /// Fuel the welder has to do tasks
        /// </summary>
        public float Fuel
        {
            get => _fuel;
            set => _fuel = value;
        }
        private float _fuel = 0;

        /// <summary>
        /// Default Cost of using the welder fuel for an action
        /// </summary>
        public const float DefaultFuelCost = 5;

        /// <summary>
        /// Rate at which we expunge fuel from ourselves when activated
        /// </summary>
        public const float FuelLossRate = 0.2f;

        /// <summary>
        /// Status of welder, whether it is ignited
        /// </summary>
        public bool Activated { get; private set; } = false;

        //private string OnSprite { get; set; }
        //private string OffSprite { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            spriteComponent = Owner.GetComponent<SpriteComponent>();
        }

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataField(ref _fuelCapacity, "Capacity", 50);
            serializer.DataField(ref _fuel, "Fuel", FuelCapacity);
        }

        public void OnUpdate(float frameTime)
        {
            if (!Activated)
            {
                return;
            }

            Fuel = Math.Max(Fuel - (FuelLossRate * frameTime), 0);

            if (Fuel == 0)
            {
                ToggleStatus();
            }
        }

        public bool TryUse(float value)
        {
            if (!Activated || !CanUse(value))
            {
                return false;
            }

            Fuel -= value;
            return true;
        }

        public bool CanUse(float value)
        {
            return Fuel > value;
        }

        public override bool CanUse()
        {
            return CanUse(DefaultFuelCost);
        }

        public bool CanActivate()
        {
            return Fuel > 0;
        }

        public bool UseEntity(IEntity user)
        {
            return ToggleStatus();
        }

        /// <summary>
        /// Deactivates welding tool if active, activates welding tool if possible
        /// </summary>
        /// <returns></returns>
        public bool ToggleStatus()
        {
            if (Activated)
            {
                Activated = false;
                // Layer 1 is the flame.
                spriteComponent.LayerSetVisible(1, false);
                return true;
            }
            else if (CanActivate())
            {
                Activated = true;
                spriteComponent.LayerSetVisible(1, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        string IExamine.Examine()
        {
            if (Activated)
            {
                return "The welding tool is currently lit";
            }
            return null;
        }
    }
}
