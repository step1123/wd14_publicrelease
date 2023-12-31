using System.Linq;
using Content.Server.Access.Systems;
using Content.Server.Construction.Completions;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Server.Mind.Components;
using Content.Server.PDA;
using Content.Server.Roles;
using Content.Shared.Access.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Preferences;

namespace Content.Server.White.Other.RandomHumanSystem;

public sealed class RandomHumanSystem : EntitySystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IdCardSystem _card = default!;
    [Dependency] private readonly PdaSystem _pda = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomHumanComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, RandomHumanComponent component, ComponentInit args)
    {
        ChangeHumanAppearance(uid,component);
    }

    public void ChangeHumanAppearance(EntityUid uid, RandomHumanComponent? randomHumanComponent = null,
        HumanoidAppearanceComponent? component = null)
    {
        if(!Resolve(uid,ref randomHumanComponent, ref component,false))
            return;

        var newProfile = HumanoidCharacterProfile.RandomWithSpecies();

        _humanoid.LoadProfile(uid, newProfile, humanoid:component);

        if (TryComp(uid, out MindContainerComponent? mindContainer) && mindContainer.HasMind &&
            mindContainer.Mind.Roles.OfType<NukeopsRole>().Any())
            return;

        _metaData.SetEntityName(uid, newProfile.Name);

        if (!_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
            return;

        if (!EntityManager.TryGetComponent(idUid, out PdaComponent? pdaComponent) || !TryComp<IdCardComponent>(pdaComponent.ContainedId, out var card))
            return;

        var cardId = pdaComponent.ContainedId.Value;

        _card.TryChangeFullName(cardId, newProfile.Name, card);
        _pda.SetOwner(idUid.Value, pdaComponent, newProfile.Name);

        _identity.QueueIdentityUpdate(uid);
    }
}
