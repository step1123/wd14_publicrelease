using Content.Shared.Examine;
using Robust.Server.Player;

namespace Content.Server.White.Other.CustomFluffSystems.merkka;

public sealed class CustomCatExamineSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CustomCatExamineComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid u, CustomCatExamineComponent comp, ExaminedEvent ev)
    {
        GetOwner(comp);

        if (comp.CatOwner == null)
            return;

        ev.PushMarkup($"Владелец: {comp.CatOwner}");
    }

    private void GetOwner(CustomCatExamineComponent comp)
    {
        if (!_playerManager.TryGetSessionByUsername("merkkaa", out var player))
            return;

        if (!TryComp<MetaDataComponent>(player.AttachedEntity, out var meta))
            return;

        comp.CatOwner = meta.EntityName;
    }
}
