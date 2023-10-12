using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Armor;

/// <summary>
/// Used for clothing that reduces damage when worn.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedArmorSystem))]
public sealed class ArmorComponent : Component
{
    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField("modifiers", required: true)]
    public DamageModifierSet Modifiers = default!;
}

/// <summary>
/// Event raised on an armor entity to get additional examine text relating to its armor.
/// </summary>
/// <param name="Msg"></param>
[ByRefEvent]
public record struct ArmorExamineEvent(FormattedMessage Msg);
