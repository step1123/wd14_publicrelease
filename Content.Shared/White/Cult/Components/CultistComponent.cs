using System.Threading;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.White.Cult.Actions;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.White.Cult;

/// <summary>
/// This is used for tagging a mob as a cultist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed class CultistComponent : Component
{
    [DataField("greetSound", customTypeSerializer: typeof(SoundSpecifierTypeSerializer))]
    public readonly SoundSpecifier? CultistGreetSound = new SoundPathSpecifier("/Audio/CultSounds/fart.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("holyConvertTime")]
    public float HolyConvertTime = 15f;

    public CancellationTokenSource? HolyConvertToken;

    [NonSerialized]
    public List<ActionType> SelectedEmpowers = new();

    public static InstantAction SummonCultDaggerAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(200),
        DisplayName = "Summon cult dagger.",
        Description = "Summons a ritual dagger.",
        Icon = new SpriteSpecifier.Texture(new ResPath("Interface/Actions/icon.png")),
        Event = new CultSummonDaggerActionEvent()
    };

    public static InstantAction BloodRitesAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(35),
        DisplayName = "Blood Rites",
        Description = "Sucks blood and heals you",
        Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/White/Cult/actions_cult.rsi"), "blood_rites"),
        Event = new CultBloodRitesInstantActionEvent()
    };

    public static EntityTargetAction CultTwistedConstructionAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(50),
        DisplayName = "Twisted Construction",
        Description = "A sinister spell that is used to turn metal into runic metal.",
        Icon = new SpriteSpecifier.Texture(new ResPath("Objects/Materials/Sheets/metal.rsi/steel.png")),
        Event = new CultTwistedConstructionActionEvent()
    };

    public static EntityTargetAction CultTeleportAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(30),
        DisplayName = "Teleport",
        CanTargetSelf = true,
        DeselectOnMiss = true,
        Repeat = false,
        Description = "A useful spell that teleports cultists to a chosen destination",
        Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/White/Cult/actions_cult.rsi"), "teleport"),
        Event = new CultTeleportTargetActionEvent(),
        Whitelist = new EntityWhitelist
        {
            Components = new[]
            {
                "HumanoidAppearance", "Cultist"
            }
        }
    };

    public static EntityTargetAction CultSummonCombatEquipmentAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(300),
        DisplayName = "Summon combat equipment",
        CanTargetSelf = true,
        DeselectOnMiss = true,
        Repeat = false,
        Description = "A crucial spell that enables you to summon a full set of combat gear",
        Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/White/Cult/actions_cult.rsi"), "armor"),
        Event = new CultSummonCombatEquipmentTargetActionEvent(),
        Whitelist = new EntityWhitelist
        {
            Components = new[]
            {
                "HumanoidAppearance", "Cultist"
            }
        }
    };

    public static List<ActionType> CultistActions = new()
    {
        SummonCultDaggerAction, BloodRitesAction, CultTwistedConstructionAction, CultTeleportAction,
        CultSummonCombatEquipmentAction
    };
}
