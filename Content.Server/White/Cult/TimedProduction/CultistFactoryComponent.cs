using Content.Server.UserInterface;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.UI;
using Robust.Server.GameObjects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.White.Cult.TimedProduction;

[RegisterComponent]
public sealed class CultistFactoryComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("cooldown")]
    public int Cooldown = 240;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? NextTimeUse;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("products", customTypeSerializer: typeof(PrototypeIdListSerializer<CultistFactoryProductionPrototype>))]
    public IReadOnlyCollection<string> Products = ArraySegment<string>.Empty;

    public BoundUserInterface? UserInterface => Owner.GetUIOrNull(CultistAltarUiKey.Key);

    [ViewVariables(VVAccess.ReadOnly)]
    public bool Active = true;
}
