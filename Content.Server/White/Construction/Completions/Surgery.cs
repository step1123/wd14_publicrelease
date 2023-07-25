using Content.Server.Body.Systems;
using Content.Server.White.Cyborg.Systems;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Construction;
using Content.Shared.White.CheapSurgery;
using Content.Shared.White.Cyborg.Components;
using Robust.Server.Containers;

namespace Content.Server.White.Construction.Completions;

public sealed class Surgery : IGraphAction
{
    private ISawmill _sawmill = default!;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        _sawmill = Logger.GetSawmill("Surgery");
        var bodySystem = entityManager.EntitySysManager.GetEntitySystem<BodySystem>();

        if (!entityManager.TryGetComponent<ActiveSurgeryComponent>(uid, out var surgeryComponent))
        {
            _sawmill.Warning($"Entity {uid} does not have a ActiveSurgery Component");
            return;
        }


        if (entityManager.TryGetComponent<OrganComponent>(surgeryComponent.OrganUid, out var organComponent))
            bodySystem.DropOrgan(surgeryComponent.OrganUid, organComponent);
        else if (entityManager.TryGetComponent<BodyPartComponent>(surgeryComponent.OrganUid, out var partComponent))
            bodySystem.DropPart(surgeryComponent.OrganUid, partComponent);

        entityManager.RemoveComponent<ActiveSurgeryComponent>(uid);
    }
}
