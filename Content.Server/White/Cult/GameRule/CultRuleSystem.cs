using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.NPC.Systems;
using Content.Server.Objectives;
using Content.Server.Players;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Content.Shared.White;
using Content.Shared.White.Cult;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.White.Cult.GameRule;

public sealed class CultRuleSystem : GameRuleSystem<CultRuleComponent>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly StorageSystem _storageSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly NpcFactionSystem _factionSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    private ISawmill _sawmill = default!;

    private int _minimalCultists;
    private int _cultGameRuleMinimapPlayers;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("preset");
        _minimalCultists = _cfg.GetCVar(WhiteCVars.CultMinStartingPlayers);
        _cultGameRuleMinimapPlayers = _cfg.GetCVar(WhiteCVars.CultMinPlayers);

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayersSpawned);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        SubscribeLocalEvent<CultNarsieSummoned>(OnNarsieSummon);

        SubscribeLocalEvent<CultistComponent, ComponentInit>(OnCultistComponentInit);
        SubscribeLocalEvent<CultistComponent, ComponentRemove>(OnCultistComponentRemoved);
        SubscribeLocalEvent<CultistComponent, MobStateChangedEvent>(OnCultistsStateChanged);

    }

    private void OnCultistsStateChanged(EntityUid uid, CultistComponent component, MobStateChangedEvent ev)
    {
        if (ev.NewMobState == MobState.Dead)
        {
            CheckRoundShouldEnd();
        }
    }

    public Mind.Mind? GetTarget()
    {
        var querry = EntityQueryEnumerator<CultRuleComponent, GameRuleComponent>();

        while (querry.MoveNext(out var ruleEnt, out var cultRuleComponent, out var gameRule))
        {
            return cultRuleComponent.CultTarget;
        }

        return null!;
    }

    public bool CanSummonNarsie()
    {
        var querry = EntityQueryEnumerator<CultRuleComponent, GameRuleComponent>();

        while (querry.MoveNext(out var ruleEnt, out var cultRuleComponent, out var gameRule))
        {
            bool enoughCultists = cultRuleComponent.Cultists.Count > 10;

            var target = cultRuleComponent.CultTarget;
            bool targetKilled = target == null || _mindSystem.IsCharacterDeadIc(target);

            if (enoughCultists && targetKilled)
                return true;
        }

        return false;
    }

    private void CheckRoundShouldEnd()
    {
        var querry = EntityQueryEnumerator<CultRuleComponent, GameRuleComponent>();
        int aliveCultistsCount = 0;

        while (querry.MoveNext(out var ruleEnt, out var cultRuleComponent, out var gameRule))
        {
            foreach (var cultistComponent in cultRuleComponent.Cultists)
            {
                var owner = cultistComponent.Owner;
                if(!TryComp<MobStateComponent>(owner, out var mobState)) continue;

                if (_mobStateSystem.IsAlive(owner, mobState))
                {
                    aliveCultistsCount++;
                }
            }
        }

        if (aliveCultistsCount == 0)
        {
            _roundEndSystem.EndRound();
        }
    }

    private void OnCultistComponentInit(EntityUid uid, CultistComponent component, ComponentInit args)
    {
        var query = EntityQueryEnumerator<CultRuleComponent, GameRuleComponent>();

        while (query.MoveNext(out var ruleEnt, out var cultRuleComponent, out var gameRule))
        {
            if(!GameTicker.IsGameRuleAdded(ruleEnt)) continue;

            cultRuleComponent.Cultists.Add(component);

            if (TryComp<ActorComponent>(component.Owner, out var actor))
            {
                cultRuleComponent.CultistsList.Add(MetaData(component.Owner).EntityName, actor.PlayerSession.Name);
            }

            UpdateCultistsAppearance(cultRuleComponent);
        }

    }

    private void OnCultistComponentRemoved(EntityUid uid, CultistComponent component, ComponentRemove args)
    {
        var query = EntityQueryEnumerator<CultRuleComponent, GameRuleComponent>();

        while (query.MoveNext(out var ruleEnt, out var cultRuleComponent, out var gameRule))
        {
            if(!GameTicker.IsGameRuleAdded(ruleEnt)) continue;

            cultRuleComponent.Cultists.Remove(component);

            RemoveCultistAppearance(component);

            CheckRoundShouldEnd();

        }

    }

    private void RemoveCultistAppearance(CultistComponent component)
    {
        if(TryComp<HumanoidAppearanceComponent>(component.Owner, out var appearanceComponent))
        {
            //Потому что я так сказал
            appearanceComponent.EyeColor = Color.White;
            Dirty(appearanceComponent);
        }

        RemComp<PentagramComponent>(component.Owner);
    }

    private void UpdateCultistsAppearance(CultRuleComponent cultRuleComponent)
    {
        var cultistsCount = cultRuleComponent.Cultists.Count;

        foreach (var cultistComponent in cultRuleComponent.Cultists)
        {
            if(cultistsCount < CultRuleComponent.ReadEyeThreshold)
                return;

            if (TryComp<HumanoidAppearanceComponent>(cultistComponent.Owner, out var appearanceComponent))
            {
                appearanceComponent.EyeColor = CultRuleComponent.EyeColor;
                Dirty(appearanceComponent);
            }

            if(cultistsCount < CultRuleComponent.PentagramThreshold)
                return;

            EnsureComp<PentagramComponent>(cultistComponent.Owner);
        }
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var querry = EntityQuery<CultRuleComponent>();

        foreach (var cultRuleComponent in querry)
        {
            var winText = Loc.GetString($"cult-cond-{cultRuleComponent.WinCondition.ToString().ToLower()}");
            ev.AddLine(winText);

            ev.AddLine(Loc.GetString("cultists-list-start"));

            foreach (var (entityName, ckey) in cultRuleComponent.CultistsList)
            {
                var lising = Loc.GetString("cultists-list-name", ("name", entityName), ("user", ckey));
                ev.AddLine(lising);
            }
        }
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {

    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        var query = EntityQueryEnumerator<CultRuleComponent, GameRuleComponent>();

        while (query.MoveNext(out var uid, out var cultRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var minPlayers = _cultGameRuleMinimapPlayers;
            if (!ev.Forced && ev.Players.Length < minPlayers)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("traitor-not-enough-ready-players",
                    ("readyPlayersCount", ev.Players.Length), ("minimumPlayers", minPlayers)));
                ev.Cancel();
                continue;
            }

            if (ev.Players.Length == 0)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("traitor-no-one-ready"));
                ev.Cancel();
            }
        }
    }

    private void OnPlayersSpawned(RulePlayerJobsAssignedEvent ev)
    {
        var query = EntityQueryEnumerator<CultRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var cultRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            foreach (var player in ev.Players)
            {
                if(!ev.Profiles.ContainsKey(player.UserId))
                    continue;

                cultRule.StarCandidates[player] = ev.Profiles[player.UserId];
            }

            var potentialCultists = FindPotentialCultist(cultRule.StarCandidates, cultRule);
            var pickedCultist = PickCultists(potentialCultists);
            var potentialTargets = FindPotentialTargets(pickedCultist);

            cultRule.CultTarget = _random.PickAndTake(potentialTargets).Mind;

            foreach (var pickerCultist in pickedCultist)
            {
                MakeCultist(pickerCultist);
            }
        }
    }

    private List<MindContainerComponent> FindPotentialTargets(List<IPlayerSession> exclude = null!)
    {
        var querry = EntityManager.EntityQuery<MindContainerComponent, HumanoidAppearanceComponent, ActorComponent>();

        var potentialTargets = new List<MindContainerComponent>();

        foreach (var (mind, humanoid, actor) in querry)
        {
            var entity = mind.Mind?.OwnedEntity;

            if (entity == default)
                continue;

            //if(!_mobStateSystem.IsAlive(entity.Value))
                //continue;

            if (exclude?.Contains(actor.PlayerSession) is true)
            {
                continue;
            }

            potentialTargets.Add(mind);
        }

        return potentialTargets;
    }

    private List<IPlayerSession> FindPotentialCultist(in Dictionary<IPlayerSession, HumanoidCharacterProfile> candidates, CultRuleComponent ruleComponent)
    {
        var list = new List<IPlayerSession>();
        var pendingQuery = GetEntityQuery<PendingClockInComponent>();

        foreach (var player in candidates.Keys)
        {
            // Role prevents antag.
            if (!(player.Data.ContentData()?.Mind?.AllRoles.All(role => role is not Job { CanBeAntag: false }) ?? false))
            {
                continue;
            }

            // Latejoin
            if (player.AttachedEntity != null && pendingQuery.HasComponent(player.AttachedEntity.Value))
                continue;

            list.Add(player);
        }

        var prefList = new List<IPlayerSession>();

        foreach (var player in list)
        {
            var profile = candidates[player];

            if (profile.AntagPreferences.Contains(CultRuleComponent.CultistPrototypeId))
            {
                prefList.Add(player);
            }
        }

        if (prefList.Count == 0)
        {
            _sawmill.Info("Insufficient preferred cultists, picking at random.");
            prefList = list;
        }

        if (prefList.Count < _minimalCultists)
        {
            var playersToAdd = _minimalCultists - prefList.Count;

            foreach (var prefPlayer in prefList)
            {
                list.Remove(prefPlayer);
            }

            for (int i = 0; i < playersToAdd; i++)
            {
                var randomPlayer = _random.PickAndTake(list);
                prefList.Add(randomPlayer);
            }
        }

        return prefList;
    }

    private List<IPlayerSession> PickCultists(List<IPlayerSession> prefList)
    {
        var result = new List<IPlayerSession>();
        if (prefList.Count == 0)
        {
            _sawmill.Info("Insufficient ready players to fill up with cultists, stopping the selection.");
            return result;
        }

        var minCultists = _cfg.GetCVar(WhiteCVars.CultMinPlayers);
        var maxCultists = _cfg.GetCVar(WhiteCVars.CultMaxStartingPlayers);

        int actualCultistCount = prefList.Count > maxCultists ? maxCultists : minCultists;


        for (int i = 0; i < actualCultistCount; i++)
        {
            result.Add(_random.PickAndTake(prefList));
        }

        return result;
    }

    public bool MakeCultist(IPlayerSession cultist)
    {
        var cultistRule = EntityQuery<CultRuleComponent>().FirstOrDefault();

        if (cultistRule == null)
        {
            GameTicker.StartGameRule(CultRuleComponent.CultGamePresetPrototype, out var ruleEntity);
            cultistRule = Comp<CultRuleComponent>(ruleEntity);
        }

        var mind = cultist.Data.ContentData()?.Mind;
        if (mind == null)
        {
            _sawmill.Info("Failed getting mind for picked cultist.");
            return false;
        }

        if (mind.OwnedEntity is not { } playerEntity)
        {
            _sawmill.Error("Mind picked for cultist did not have an attached entity.");
            return false;
        }

        DebugTools.AssertNotNull(mind.OwnedEntity);
        EnsureComp<CultistComponent>(playerEntity);

        _factionSystem.RemoveFaction(playerEntity, "NanoTrasen", false);
        _factionSystem.AddFaction(playerEntity, "Cultist");

        if (_inventorySystem.TryGetSlotEntity(playerEntity, "back", out var backPack))
        {
            foreach (var itemPrototype in cultistRule.StartingItems)
            {
                var itemEntity = Spawn(itemPrototype, Transform(playerEntity).Coordinates);

                if (backPack != null)
                {
                    _storageSystem.Insert(backPack.Value, itemEntity);
                }
            }
        }

        _audioSystem.PlayGlobal(cultistRule.GreatingsSound, Filter.Empty().AddPlayer(cultist), false, AudioParams.Default);
        _chatManager.DispatchServerMessage(cultist, Loc.GetString("cult-role-greeting"));

        if (_prototypeManager.TryIndex<ObjectivePrototype>("CultistKillObjective", out var cultistObjective))
        {
            _mindSystem.TryAddObjective(mind, cultistObjective);
        }

        return true;
    }

    private void OnNarsieSummon(CultNarsieSummoned ev)
    {
        var query = EntityQuery<MobStateComponent, MindContainerComponent, CultistComponent>().ToList();

        foreach (var (mobState, mindContainer, cultistComponent) in query)
        {
            if(mindContainer.HasMind && mindContainer.Mind is not null)
            {
                var reaper = Spawn(CultRuleComponent.ReaperPrototype, Transform(mobState.Owner).Coordinates);
                _mindSystem.TransferTo(mindContainer.Mind, reaper);

                _bodySystem.GibBody(mobState.Owner);
            }
        }

        _roundEndSystem.EndRound();
    }

    protected override void Added(EntityUid uid, CultRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);
    }

    protected override void Ended(EntityUid uid, CultRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
    }

    protected override void Started(EntityUid uid, CultRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
    }
}
