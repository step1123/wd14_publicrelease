﻿using SS14.Shared.GameObjects;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.IoC;
using SS14.Shared.Utility;
using System;
using YamlDotNet.RepresentationModel;

namespace Content.Server.GameObjects.Components.Power
{
    /// <summary>
    /// Feeds energy from the powernet and may have the ability to supply back into it
    /// </summary>
    public class PowerStorageComponent : Component
    {
        public override string Name => "PowerStorage";

        /// <summary>
        /// Maximum amount of energy the internal battery can store
        /// </summary>
        public float Capacity { get; private set; } = 10000; //arbitrary value replace

        /// <summary>
        /// Energy the battery is currently storing
        /// </summary>
        public float Charge { get; private set; } = 0;

        /// <summary>
        /// Rate at which energy will be taken to charge internal battery
        /// </summary>
        public float ChargeRate { get; private set; } = 1000;

        /// <summary>
        /// Rate at which energy will be distributed to the powernet if needed
        /// </summary>
        public float DistributionRate { get; private set; } = 1000;
        
        private bool _chargepowernet = false;

        /// <summary>
        /// Do we distribute power into the powernet from our stores if the powernet requires it?
        /// </summary>
        public bool ChargePowernet
        {
            get => _chargepowernet;
            set
            {
                _chargepowernet = value;
                if (Owner.TryGetComponent(out PowerNodeComponent node))
                {
                    if (node.Parent != null)
                        node.Parent.UpdateStorageType(this);
                }
            }
        }


        public override void LoadParameters(YamlMappingNode mapping)
        {
            if (mapping.TryGetNode("Capacity", out YamlNode node))
            {
                Capacity = node.AsFloat();
            }
            if (mapping.TryGetNode("Charge", out node))
            {
                Charge = node.AsFloat();
            }
            if (mapping.TryGetNode("ChargeRate", out node))
            {
                ChargeRate = node.AsFloat();
            }
            if (mapping.TryGetNode("DistributionRate", out node))
            {
                DistributionRate = node.AsFloat();
            }
            if (mapping.TryGetNode("ChargePowernet", out node))
            {
                _chargepowernet = node.AsBool();
            }
        }

        public override void OnAdd()
        {
            base.OnAdd();

            if (!Owner.TryGetComponent(out PowerNodeComponent node))
            {
                Owner.AddComponent<PowerNodeComponent>();
            }
            node.OnPowernetConnect += PowernetConnect;
            node.OnPowernetDisconnect += PowernetDisconnect;
            node.OnPowernetRegenerate += PowernetRegenerate;
        }

        public override void OnRemove()
        {
            if (Owner.TryGetComponent(out PowerNodeComponent node))
            {
                if (node.Parent != null)
                {
                    node.Parent.RemovePowerStorage(this);
                }

                node.OnPowernetConnect -= PowernetConnect;
                node.OnPowernetDisconnect -= PowernetDisconnect;
                node.OnPowernetRegenerate -= PowernetRegenerate;
            }

            base.OnRemove();
        }

        /// <summary>
        /// Checks if the storage can supply the amount of charge directly requested
        /// </summary>
        public bool CanDeductCharge(float todeduct)
        {
            if (Charge > todeduct)
                return true;
            return false;
        }

        /// <summary>
        /// Deducts the requested charge from the energy storage
        /// </summary>
        public void DeductCharge(float todeduct)
        {
            Charge = Math.Min(0, Charge - todeduct);
        }

        /// <summary>
        /// Returns all possible charge available from the energy storage
        /// </summary>
        public float RequestAllCharge()
        {
            return Math.Min(ChargeRate, Capacity - Charge);
        }

        /// <summary>
        /// Returns the charge available from the energy storage
        /// </summary>
        public float RequestCharge()
        {
            return Math.Min(ChargeRate, Capacity - Charge);
        }

        /// <summary>
        /// Returns the charge available from the energy storage
        /// </summary>
        public float AvailableCharge()
        {
            return Math.Min(DistributionRate, Charge);
        }

        /// <summary>
        /// Gives the storage one full tick of charging its energy storage
        /// </summary>
        public void ChargePowerTick()
        {
            Charge = Math.Max(Charge + ChargeRate, Capacity);
        }

        /// <summary>
        /// Takes from the storage one full tick of energy
        /// </summary>
        public void RetrievePassiveStorage()
        {
            Charge = Math.Min(Charge - DistributionRate, 0);
        }

        /// <summary>
        /// Node has become anchored to a powernet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventarg"></param>
        private void PowernetConnect(object sender, PowernetEventArgs eventarg)
        {
            eventarg.Powernet.AddPowerStorage(this);
        }

        /// <summary>
        /// Node has had its powernet regenerated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventarg"></param>
        private void PowernetRegenerate(object sender, PowernetEventArgs eventarg)
        {
            eventarg.Powernet.AddPowerStorage(this);
        }

        /// <summary>
        /// Node has become unanchored from a powernet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventarg"></param>
        private void PowernetDisconnect(object sender, PowernetEventArgs eventarg)
        {
            eventarg.Powernet.RemovePowerStorage(this);
        }
    }
}
