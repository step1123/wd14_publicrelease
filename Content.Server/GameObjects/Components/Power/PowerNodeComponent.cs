﻿using SS14.Server.GameObjects;
using SS14.Server.Interfaces.GameObjects;
using SS14.Shared.GameObjects;
using SS14.Shared.IoC;
using System;
using System.Linq;

namespace Content.Server.GameObjects.Components.Power
{
    /// <summary>
    /// Component that connects to the powernet
    /// </summary>
    public class PowerNodeComponent : Component
    {
        public override string Name => "PowerNode";
        
        /// <summary>
        /// The powernet this node is connected to
        /// </summary>
        public Powernet Parent;

        /// <summary>
        /// An event handling when this node connects to a powernet
        /// </summary>
        public event EventHandler<PowernetEventArgs> OnPowernetConnect;

        /// <summary>
        /// An event handling when this node disconnects from a powernet
        /// </summary>
        public event EventHandler<PowernetEventArgs> OnPowernetDisconnect;

        /// <summary>
        /// An event that registers us to a regenerating powernet
        /// </summary>
        public event EventHandler<PowernetEventArgs> OnPowernetRegenerate;

        public override void Initialize()
        {
            TryCreatePowernetConnection();
        }

        public override void OnRemove()
        {
            DisconnectFromPowernet();

            base.OnRemove();
        }

        /// <summary>
        /// Find a nearby wire which will have a powernet and connect ourselves to its powernet
        /// </summary>
        public void TryCreatePowernetConnection()
        {
            var _emanager = IoCManager.Resolve<IServerEntityManager>();
            var position = Owner.GetComponent<TransformComponent>().WorldPosition;
            var wires = _emanager.GetEntitiesIntersecting(Owner)
                        .Where(x => x.HasComponent<PowerTransferComponent>())
                        .OrderByDescending(x => (x.GetComponent<TransformComponent>().WorldPosition - position).Length);
            var choose = wires.FirstOrDefault();
            if(choose != null)
                ConnectToPowernet(choose.GetComponent<PowerTransferComponent>().Parent);
        }

        /// <summary>
        /// Triggers event telling power components that we connected to a powernet
        /// </summary>
        /// <param name="toconnect"></param>
        public void ConnectToPowernet(Powernet toconnect)
        {
            Parent = toconnect;
            Parent.Nodelist.Add(this);
            OnPowernetConnect?.Invoke(this, new PowernetEventArgs(Parent));
        }

        /// <summary>
        /// Triggers event telling power components that we haven't disconnected but have readded ourselves to a regenerated powernet
        /// </summary>
        /// <param name="toconnect"></param>
        public void RegeneratePowernet(Powernet toconnect)
        {
            //This removes the device from things that will be powernet disconnected when dirty powernet is killed
            Parent.Nodelist.Remove(this);

            Parent = toconnect;
            Parent.Nodelist.Add(this);
            OnPowernetRegenerate?.Invoke(this, new PowernetEventArgs(Parent));
        }

        /// <summary>
        /// Triggers event telling power components we have exited any powernets
        /// </summary>
        public void DisconnectFromPowernet()
        {
            Parent.Nodelist.Remove(this);
            OnPowernetDisconnect?.Invoke(this, new PowernetEventArgs(Parent));
            Parent = null;
        }
    }

    public class PowernetEventArgs : EventArgs
    {
        public PowernetEventArgs(Powernet powernet)
        {
            Powernet = powernet;
        }

        public Powernet Powernet { get; }
    }
}
