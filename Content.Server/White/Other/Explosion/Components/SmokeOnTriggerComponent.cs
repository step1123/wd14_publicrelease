using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Audio;

namespace Content.Server.White.Other.Explosion.Components;

[RegisterComponent]
[Access(typeof(SmokeSystem))]
public sealed class SmokeOnTriggerComponent : Component
{
    [DataField("spreadAmount"), ViewVariables(VVAccess.ReadWrite)]
    public int SpreadAmount = 20;

    [DataField("time"), ViewVariables(VVAccess.ReadWrite)]
    public float Time = 20f;

    [DataField("smokeReagents")] public List<Solution.ReagentQuantity> SmokeReagents = new();

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/smoke.ogg");
}
