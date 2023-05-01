using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Utility;

namespace Content.Server.White.Other.Lazy;

[RegisterComponent]
public sealed class EarsSpawnComponent : Component
{
    [DataField("summonAction")] public InstantAction SummonAction = new()
    {
        Icon = new SpriteSpecifier.Texture(new ResPath("Clothing/Head/Hats/witch.rsi/icon.png")),
        DisplayName = "summon cat ears",
        Description = "meow!",
        Event = new SummonActionEarsEvent()
    };
}
