using Content.Shared.Access.Components;
using Content.Shared.Examine;
using Robust.Shared.Enums;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.White;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.White.Other.ExamineSystem
{

    //^.^
    public sealed class ExamineSystem : EntitySystem
    {
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly IConsoleHost _consoleHost = default!;
        [Dependency] private readonly INetConfigurationManager _netConfigManager = default!;


        public override void Initialize()
        {
            SubscribeLocalEvent<ExaminableClothesComponent, ExaminedEvent>(HandleExamine);
        }

        private void SendNoticeMessage(ActorComponent actorComponent, string message)
        {
            var should = _netConfigManager.GetClientCVar(actorComponent.PlayerSession.ConnectedClient, WhiteCVars.LogChatActions);

            if (should)
            {
                _consoleHost.RemoteExecuteCommand(actorComponent.PlayerSession, $"notice [font size=10][color=#aeabc4]{message}[/color][/font]");
            }
        }

        private void HandleExamine(EntityUid uid, ExaminableClothesComponent comp, ExaminedEvent args)
        {
            var infoLines = new List<string>();

            if (TryComp<ActorComponent>(args.Examiner, out var actorComponent) &&
                TryComp<MetaDataComponent>(uid, out var metaDataComponent))
            {
                infoLines.Add($"Это же [bold]{metaDataComponent.EntityName}[/bold]!");
            }

            var idInfoString = GetInfo(uid);
            if (!string.IsNullOrEmpty(idInfoString))
            {
                infoLines.Add(idInfoString);
                args.PushMarkup(idInfoString);
            }

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
                {
                    var item = $"{Loc.GetString(slotLabel)} [bold]{metaData.EntityName}[/bold].";
                    args.PushMarkup(item);
                    infoLines.Add(item);
                }
            }

            var combinedInfo = string.Join("\n", infoLines);

            if (actorComponent != null)
            {
                SendNoticeMessage(actorComponent, combinedInfo);
            }
        }

        private string GetInfo(EntityUid uid)
        {
            if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
            {
                // PDA
                if (EntityManager.TryGetComponent(idUid, out PdaComponent? pda) &&
                    TryComp<IdCardComponent>(pda.ContainedId, out var id))
                {
                    return GetNameAndJob(id);
                }
                // ID Card
                if (EntityManager.TryGetComponent(idUid, out id))
                {
                    return GetNameAndJob(id);
                }
            }
            return "";
        }

        private string GetNameAndJob(IdCardComponent id)
        {
            var jobSuffix = string.IsNullOrWhiteSpace(id.JobTitle) ? "" : $" ({id.JobTitle})";

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
