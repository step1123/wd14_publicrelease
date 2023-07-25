﻿using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cult;

[Serializable, NetSerializable]
public enum NarsieVisualState : byte
{
    VisualState
}

[Serializable, NetSerializable]
public enum NarsieVisuals : byte
{
    Spawning,
    Spawned
}


[RegisterComponent, NetworkedComponent]
public partial class NarsieComponent : Component
{
}

