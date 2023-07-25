﻿using System.Threading;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cult.Items;

[RegisterComponent]
public sealed class VoidTeleportComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("usesLeft")]
    public int UsesLeft = 4;

    [ViewVariables(VVAccess.ReadWrite), DataField("minRange")]
    public int MinRange = 5;

    [ViewVariables(VVAccess.ReadWrite), DataField("maxRange")]
    public int MaxRange = 15;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool Active = true;

    [ViewVariables(VVAccess.ReadWrite), DataField("cooldown")]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(5);

    public CancellationTokenSource Token = new();

    public TimeSpan TimerDelay = TimeSpan.FromSeconds(0.5);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextUse = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite), DataField("teleportInSound")]
    public SoundSpecifier TeleportInSound = new SoundPathSpecifier("/Audio/White/Cult/veilin.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("teleportOutSound")]
    public SoundSpecifier TeleportOutSound = new SoundPathSpecifier("/Audio/White/Cult/veilout.ogg");

    [ViewVariables(VVAccess.ReadOnly), DataField("teleportInEffect")]
    public string? TeleportInEffect = "CultTeleportInEffect";

    [ViewVariables(VVAccess.ReadOnly), DataField("teleportOutEffect")]
    public string? TeleportOutEffect = "CultTeleportOutEffect";
}

[Serializable, NetSerializable]
public enum VeilVisuals : byte
{
    Activated
}
