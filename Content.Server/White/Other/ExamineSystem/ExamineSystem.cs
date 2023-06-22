using Content.Shared.Access.Components;
using Content.Shared.Examine;
using Robust.Shared.Enums;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.PDA;

namespace Content.Server.White.Other.ExamineSystem
{
    public sealed class ExamineSystem : EntitySystem
    {
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<ExaminableClothesComponent, ExaminedEvent>(HandleExamine);
        }

        private void HandleExamine(EntityUid uid, ExaminableClothesComponent comp, ExaminedEvent args)
        {
            var slotLabels = new Dictionary<string, string>
            {
                { "head", "head-" },
                { "eyes", "eyes-" },
                { "mask", "mask-" },
                { "neck", "neck-" },
                { "ears", "ears-" },
                { "jumpsuit", "jumpsuit-" },
                { "outerClothing", "outer-" },
                { "back", "back-" },
                { "gloves", "gloves-" },
                { "belt", "belt-" },
                { "shoes", "shoes-" }
            };

            foreach (var slotEntry in slotLabels)
            {
                var slotName = slotEntry.Key;
                var slotLabel = slotEntry.Value;

                if (_entityManager.TryGetComponent<HumanoidAppearanceComponent>(uid, out var appearanceComponent))
                {
                    switch (appearanceComponent.Gender)
                    {
                        case Gender.Male:
                            slotLabel += "he";
                            break;
                        case Gender.Neuter:
                            slotLabel += "it";
                            break;
                        case Gender.Epicene:
                            slotLabel += "they";
                            break;
                        case Gender.Female:
                            slotLabel += "she";
                            break;
                    }
                }

                if (!_inventorySystem.TryGetSlotEntity(uid, slotName, out var slotEntity))
                    continue;

                if (_entityManager.TryGetComponent<MetaDataComponent>(slotEntity, out var metaData))
                    args.PushMarkup($"[color=silver]{Loc.GetString(slotLabel)} [/color][font size=11][bold][color=lightgray]{metaData.EntityName}[/color][/bold][/font].");
            }

            var id = GetInfo(uid);
            if (id != null)
                args.PushMarkup(id);
        }

        private string? GetInfo(EntityUid uid)
        {
            if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
            {
                // PDA
                if (EntityManager.TryGetComponent(idUid, out PdaComponent? pda) && pda.ContainedId is not null)
                {
                    return GetNameAndJob(pda.ContainedId);
                }
                // ID Card
                if (EntityManager.TryGetComponent(idUid, out IdCardComponent? id))
                {
                    return GetNameAndJob(id);
                }
            }
            return null;
        }

        private string GetNameAndJob(IdCardComponent id)
        {
            var jobSuffix = string.IsNullOrWhiteSpace(id.JobTitle) ? string.Empty : $" ({id.JobTitle})";

            var val = string.IsNullOrWhiteSpace(id.FullName)
                ? Loc.GetString("access-id-card-component-owner-name-job-title-text",
                    ("jobSuffix", jobSuffix))
                : Loc.GetString("access-id-card-component-owner-full-name-job-title-text",
                    ("fullName", id.FullName),
                    ("jobSuffix", jobSuffix));

            return val;
        }
    }
}
