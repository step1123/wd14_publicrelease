using Content.Server.Worldgen.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Worldgen.Systems;

public sealed class BlueprintPlacerSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BlueprintPlacerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, BlueprintPlacerComponent component, MapInitEvent args)
    {
        var xform = Transform(uid);
        var options = new MapLoadOptions()
        {
            LoadMap = false,
            Offset = xform.WorldPosition,
            Rotation = xform.LocalRotation,
        };

        _mapLoader.TryLoad(xform.MapID, component.Blueprint.ToString(), out var root, options);

        if (root is null)
            return;

        component.Apply(root[0], _serialization, EntityManager, _componentFactory);
    }
}
