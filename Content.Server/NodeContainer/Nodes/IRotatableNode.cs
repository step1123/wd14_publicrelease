﻿using Robust.Shared.GameObjects;

namespace Content.Server.NodeContainer.Nodes
{
    /// <summary>
    ///     A <see cref="Node"/> that implements this will have its <see cref="RotateEvent(RotateEvent)"/> called when its
    ///     <see cref="NodeContainerComponent"/> is rotated.
    /// </summary>
    public interface IRotatableNode
    {
        /// <summary>
        ///     Rotates this <see cref="Node"/>. Returns true if the node's connections need to be updated.
        /// </summary>
        bool RotateEvent(ref RotateEvent ev);
    }
}
