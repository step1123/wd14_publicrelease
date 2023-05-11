using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Utility;

namespace Content.Server.White.Other.CustomFluffSystems.merkka;

[RegisterComponent]
public sealed class EarsSpawnComponent : Component
{
    public InstantAction SummonAction = new()
    {
        Icon = new SpriteSpecifier.Texture(new ResPath("Clothing/Head/Hats/witch.rsi/icon.png")),
        DisplayName = "summon cat ears",
        Description = "meow!",
        Event = new SummonActionEarsEvent()
    };

    public InstantAction SummonActionCat = new()
    {
        Icon = new SpriteSpecifier.Texture(new ResPath("Clothing/Head/Hats/witch.rsi/icon.png")),
        DisplayName = "summon cat",
        Description = "meow!",
        Event = new SummonActionCatEvent()
    };

    public int CatEarsUses = 30;
    public int СatSpawnUses = 20;
}
