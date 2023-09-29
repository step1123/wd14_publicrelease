using Content.Shared.Actions;

namespace Content.Shared.White.Cult;

public sealed class ArtificerCreateSoulStoneActionEvent : InstantActionEvent
{
    public string SoulStonePrototypeId => "SoulShardGhost";
}

public sealed class ArtificerCreateConstructShellActionEvent : InstantActionEvent
{
    public string ShellPrototypeId => "ConstructShell";
}

public sealed class ArtificerConvertCultistFloorActionEvent : InstantActionEvent
{
    public string FloorTileId => "CultFloor";
}

public sealed class ArtificerCreateCultistWallActionEvent : InstantActionEvent
{
    public string WallPrototypeId => "WallCult";
}

public sealed class ArtificerCreateCultistAirlockActionEvent : InstantActionEvent
{
    public string AirlockPrototypeId => "AirlockGlassCult";
}

public sealed class WraithPhaseActionEvent : InstantActionEvent
{
    [DataField("duration")]
    public float Duration = 5f;

    public string StatusEffectId => "Incorporeal";
}

public sealed class JuggernautCreateWallActionEvent : InstantActionEvent
{
    public string WallPrototypeId = "WallInvisible";
}

