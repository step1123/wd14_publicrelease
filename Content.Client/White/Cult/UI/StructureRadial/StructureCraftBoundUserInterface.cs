using Content.Client.Construction;
using Content.Client.White.UserInterface.Controls;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Popups;
using Content.Shared.White.Cult.Structures;
using Robust.Client.GameObjects;
using Robust.Client.Input;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Cult.UI.StructureRadial;

public sealed class StructureCraftBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlacementManager _placement = default!;
    [Dependency] private readonly IEntitySystemManager _systemManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    private RadialContainer? _radialContainer;

    public StructureCraftBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    private void CreateUI()
    {
        if (_radialContainer != null)
            ResetUI();

        _radialContainer = new RadialContainer();

        foreach (var prototype in _prototypeManager.EnumeratePrototypes<CultStructurePrototype>())
        {
            var radialButton = _radialContainer.AddButton(prototype.StructureName, prototype.Icon);
            radialButton.Controller.OnPressed += _ =>
            {
                Select(prototype.StructureId);
            };
        }

        _radialContainer.OpenAttachedLocalPlayer();
    }

    private void ResetUI()
    {
        _radialContainer?.Close();
        _radialContainer = null;
    }

    protected override void Open()
    {
        base.Open();

        CreateUI();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ResetUI();
    }

    private void Select(string id)
    {
        CreateBlueprint(id);
        ResetUI();
        Close();
    }

    private void CreateBlueprint(string id)
    {
        var newObj = new PlacementInformation
        {
            Range = 2,
            IsTile = false,
            EntityType = id,
            PlacementOption = "SnapgridCenter"
        };

        _prototypeManager.TryIndex<ConstructionPrototype>(id, out var construct);

        if (construct == null)
            return;

        var player = _player.LocalPlayer?.ControlledEntity;

        if (player == null)
            return;

        if (construct.ID == "CultPylon" && CheckForStructure(player, id))
        {
            var popup = _entMan.System<SharedPopupSystem>();
            popup.PopupClient(Loc.GetString("cult-structure-craft-another-structure-nearby"), player.Value, player.Value);
            return;
        }

        var constructSystem = _systemManager.GetEntitySystem<ConstructionSystem>();
        var hijack = new ConstructionPlacementHijack(constructSystem, construct);

        _placement.BeginPlacing(newObj, hijack);
    }

    private bool CheckForStructure(EntityUid? uid, string id)
    {
        if (uid == null)
            return false;

        if (!_entMan.TryGetComponent<TransformComponent>(uid, out var transform))
            return false;

        var lookupSystem = _entMan.System<EntityLookupSystem>();
        var entities = lookupSystem.GetEntitiesInRange(transform.Coordinates, 15f);
        foreach (var ent in entities)
        {
            if (!_entMan.TryGetComponent<MetaDataComponent>(ent, out var metadata))
                continue;

            if (metadata.EntityPrototype?.ID == id)
                return true;
        }

        return false;
    }
}
