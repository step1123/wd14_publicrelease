using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Utility;

namespace Content.Server.White.Other.CustomFluffSystems.Pets;

[RegisterComponent]
public sealed class PetSummonComponent : Component
{
    public InstantAction PetSummonAction = new()
    {
        Icon = new SpriteSpecifier.Texture(new ResPath("Objects/Misc/books.rsi/summon_book.png")),
        DisplayName = "Призыв",
        Description = "Призыв питомца БЕЗ возможности вселения призрака",
        Event = new PetSummonActionEvent()
    };

    public InstantAction PetGhostSummonAction = new()
    {
        Icon = new SpriteSpecifier.Texture(new ResPath("Mobs/Ghosts/ghost_human.rsi/icon.png")),
        DisplayName = "Призрачный призыв",
        Description = "Призыв питомца С возможностью вселения призрака",
        Event = new PetGhostSummonActionEvent()
    };

    public int UsesLeft = 10;

    public EntityUid? SummonedEntity;
}
