using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Cloning;
using Content.Server.Flash;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.NPC.Systems;
using Content.Server.Players;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Storage.EntitySystems;
using Content.Server.Traits.Assorted;
using Content.Server.White.Mindshield;
using Content.Server.White.Reva.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Roles;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Mindshield;
using Content.Shared.White.Reva.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using WinCondition = Content.Server.White.Reva.Components.WinCondition;
using WinType = Content.Server.White.Reva.Components.WinType;

namespace Content.Server.White.Reva.Systems;

public sealed class RevolutionaryRuleSystem : GameRuleSystem<RevolutionaryRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly MindShieldSystem _mindShieldSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly StorageSystem _storageSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayersSpawned);
        SubscribeLocalEvent<MindContainerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<RevolutionaryComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<RevolutionaryComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
        SubscribeLocalEvent<FlashAttemptEvent>(OnFlashAttempt);
        SubscribeLocalEvent<RevolutionaryComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<MindShieldImplanted>(OnMindshieldImplanted);
        SubscribeLocalEvent<RevolutionaryComponent, CloningEvent>(OnCloned);
        SubscribeLocalEvent<RevolutionaryRuleComponent, ComponentInit>(OnRuleComponentInit);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RevolutionaryRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out var revaRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (revaRule.NextTimeCheckConditions <= _gameTiming.CurTime)
                CheckRoundShouldEnd();
        }
    }

    private void OnRuleComponentInit(EntityUid uid, RevolutionaryRuleComponent component, ComponentInit args)
    {
        if (component.HeadPlayers.Count != 0)
            return;

        var minds = EntityQuery<MindContainerComponent>();
        var headPrototypes = GetHeadsPrototypes();
        foreach (var mind in minds)
        {
            if (!mind.HasMind)
                continue;

            var jobPrototype = mind.Mind?.CurrentJob?.Prototype;
            if (jobPrototype != null && headPrototypes.Contains(jobPrototype))
            {
                if (mind.Mind?.Session != null)
                {
                    component.HeadPlayers.Add(mind.Mind?.Session!);
                    component.ScoreHeadPlayers.Add($"{mind.Mind?.CharacterName} ({mind.Mind?.Session.Name})");
                }
            }
        }

        if (component.NextTimeCheckConditions == TimeSpan.Zero)
            component.NextTimeCheckConditions += _gameTiming.CurTime + TimeSpan.FromSeconds(180);
    }

    private void OnCloned(EntityUid uid, RevolutionaryComponent component, ref CloningEvent args)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent>();
        while (query.MoveNext(out var revaRule))
        {
            if (TryComp<MindContainerComponent>(args.Source, out var comp) && comp.HasMind)
            {
                var playerSession = comp.Mind?.Session;
                if (playerSession != null)
                    revaRule.RevPlayers.Remove(playerSession);

                var mindSystem = EntityManager.EntitySysManager.GetEntitySystem<MindSystem>();
                if (mindSystem.HasRole<TraitorRole>(comp.Mind!))
                {
                    foreach (var role in comp.Mind!.AllRoles )
                    {
                        var antagPrototype = _prototypeManager.Index<AntagPrototype>(revaRule.RevaRoleProto);

                        if (role is TraitorRole traitorRole && traitorRole.Prototype.ID == antagPrototype.ID)
                        {
                            mindSystem.RemoveRole(comp.Mind!, role);
                        }
                    }
                }
                _faction.AddFaction(uid, "NanoTrasen");
            }
        }
    }

    private void OnMindshieldImplanted(MindShieldImplanted ev)
    {
        if (!TryComp<RevolutionaryComponent>(ev.Target, out var revolutionaryComponent))
            return;

        if (!revolutionaryComponent.HeadRevolutionary)
        {
            RemComp(ev.Target, revolutionaryComponent);
            if(!TryComp<ActorComponent>(ev.Target, out var actorComponent))
                return;

            _chatManager.DispatchServerMessage(actorComponent.PlayerSession, Loc.GetString("rev-mindshield-implanted-rev"));
        }
        else
        {
            _mindShieldSystem.RemoveMindShieldImplant(ev.Target, ev.MindShield, true);
        }
    }

    private void OnGetState(EntityUid uid, RevolutionaryComponent component, ref ComponentGetState args)
    {
        args.State = new RevolutionaryComponentState(component.HeadRevolutionary);
    }

    private void OnFlashAttempt(FlashAttemptEvent msg)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if(msg.User == null)
                return;
            if(msg.MassFlash)
                return;
            if(msg.Cancelled)
                return;
            if(!TryComp<ActorComponent>(msg.Target, out var actor))
                return;
            if(HasComp<CyborgComponent>(msg.Target))
                return;
            if(!TryComp<RevolutionaryComponent>(msg.User, out var comp))
                return;
            if(!comp.HeadRevolutionary)
                return;
            if (TryComp<MindContainerComponent>(msg.Target, out var mindComponent) && mindComponent.HasMind
                && GetHeadsPrototypes().Contains(mindComponent.Mind!.CurrentJob!.Prototype))
                return;
            if(HasComp<MindShieldComponent>(msg.Target))
                return;
            if(HasComp<RevolutionaryComponent>(msg.Target))
                return;

            var targetComp = EnsureComp<RevolutionaryComponent>(msg.Target);
            targetComp.HeadRevolutionary = false;
            Dirty(targetComp);

            _chatManager.DispatchServerMessage(actor.PlayerSession, Loc.GetString("rev-welcome-rev"));
        }
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out var revaRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (ev.JobId == null || !_prototypeManager.TryIndex<JobPrototype>(ev.JobId, out var job))
                return;

            if (GetHeadsPrototypes().Contains(job))
            {
                revaRule.HeadPlayers.Add(ev.Player);
                if (ev.Player.AttachedEntity != null)
                    revaRule.ScoreHeadPlayers.Add($"{MetaData(ev.Player.AttachedEntity.Value).EntityName} ({ev.Player.Name})");
                return;
            }

            if (revaRule.TotalHeadRevs >= revaRule.MaxHeadRev)
                return;
            if (!ev.LateJoin)
                return;

            if (!ev.Profile.AntagPreferences.Contains(revaRule.RevaRoleProto))
                return;
            if (!job.CanBeAntag)
                return;

            var target = ((revaRule.PlayersPerHeadRev * revaRule.TotalHeadRevs) + 1);

            var chance = (1f / revaRule.PlayersPerHeadRev);

            if (ev.JoinOrder < target)
            {
                chance /= (target - ev.JoinOrder);
            }
            else
            {
                chance *= ((ev.JoinOrder + 1) - target);
            }
            if (chance > 1)
                chance = 1;

            if (_random.Prob(chance))
            {
                MakeHeadRevolution(ev.Player, revaRule);
            }
        }
    }

    private void OnComponentRemove(EntityUid uid, RevolutionaryComponent component, ComponentRemove args)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent>();
        while (query.MoveNext(out var revaRule))
        {
            if (TryComp<MindContainerComponent>(uid, out var comp) && comp.HasMind)
            {
                var playerSession = comp.Mind?.Session;
                if (playerSession != null)
                    revaRule.RevPlayers.Remove(playerSession);

                var mindSystem = EntityManager.EntitySysManager.GetEntitySystem<MindSystem>();
                if (mindSystem.HasRole<TraitorRole>(comp.Mind!))
                {
                    foreach (var role in comp.Mind!.AllRoles )
                    {
                        var antagPrototype = _prototypeManager.Index<AntagPrototype>(revaRule.RevaRoleProto);

                        if (role is TraitorRole traitorRole && traitorRole.Prototype.ID == antagPrototype.ID)
                        {
                            mindSystem.RemoveRole(comp.Mind!, role);
                        }
                    }
                }
                _faction.AddFaction(uid, "NanoTrasen");
            }
            CheckRoundShouldEnd();
        }
    }

    private void OnComponentInit(EntityUid uid, RevolutionaryComponent component, ComponentInit args)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out var revaRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!TryComp<MindContainerComponent>(uid, out var mindComponent))
                return;

            if (!mindComponent.HasMind)
                return;

            var session = mindComponent.Mind?.Session;
            if (session != null)
            {
                revaRule.RevPlayers.Add(session);
                revaRule.ScoreRevPlayers.Add($"{mindComponent.Mind?.CharacterName} ({session.Name})", false);
            }

            var mindSystem = EntityManager.EntitySysManager.GetEntitySystem<MindSystem>();
            var antagPrototype = _prototypeManager.Index<AntagPrototype>(revaRule.RevaRoleProto);
            var traitorRole = new TraitorRole(mindComponent.Mind!, antagPrototype);

            if (mindComponent.Mind != null)
                mindSystem.AddRole(mindComponent.Mind, traitorRole);
            _faction.RemoveFaction(uid, "NanoTrasen");
            RemComp<PacifistComponent>(uid);
            RemComp<PacifiedComponent>(uid);
        }
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent>();
        while (query.MoveNext(out var revaRule))
        {
            switch (ev.New)
            {
                case GameRunLevel.InRound:
                    OnRoundStart(revaRule);
                    break;
            }
        }
    }

    private void OnRoundStart(RevolutionaryRuleComponent revaRule)
    {
        var filter = Filter.Empty();
        foreach (var headrev in EntityQuery<RevolutionaryComponent>())
        {
            if (!TryComp<ActorComponent>(headrev.Owner, out var actor))
                continue;

            filter.AddPlayer(actor.PlayerSession);
        }

        _audioSystem.PlayGlobal(revaRule.GreetSound, filter, recordReplay: false);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        foreach (var revaRule in EntityQuery<RevolutionaryRuleComponent>())
        {
            var winText = Loc.GetString($"rev-{revaRule.WinType.ToString().ToLower()}");

            ev.AddLine(winText);

            foreach (var cond in revaRule.WinConditions)
            {
                var text = Loc.GetString($"rev-cond-{cond.ToString().ToLower()}");

                ev.AddLine(text);
            }

            string listing;
            ev.AddLine(Loc.GetString("rev-list-revs-start"));
            foreach (var (name, headRev) in revaRule.ScoreRevPlayers)
            {
                listing = Loc.GetString("rev-list", ("name", name), ("headrev", headRev ? Loc.GetString("rev-list-headrevbool") : ""));
                ev.AddLine(listing);
            }
            ev.AddLine(Loc.GetString("rev-list-heads-start"));
            foreach (var name in revaRule.ScoreHeadPlayers)
            {
                listing = Loc.GetString("rev-list", ("name", name), ("headrev", ""));
                ev.AddLine(listing);
            }
        }
    }

    private void OnMobStateChanged(EntityUid uid, MindContainerComponent component, MobStateChangedEvent ev)
    {
        if(ev.NewMobState == MobState.Dead)
            CheckRoundShouldEnd();
    }

    private void CheckRoundShouldEnd()
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var revaRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var headRevsAlive = false;
            foreach (var headRev in revaRule.RevHeadPlayers)
            {
                if (headRev.AttachedEntity == null)
                    continue;

                if (TryComp<RevolutionaryComponent>(headRev.AttachedEntity, out var revComp) && revComp.HeadRevolutionary
                    && TryComp<MobStateComponent>(headRev.AttachedEntity, out var stateComp) && stateComp.CurrentState != MobState.Dead
                    && TryComp<MindContainerComponent>(headRev.AttachedEntity, out var mindComp) && mindComp.HasMind)
                {
                    headRevsAlive = true;
                    break;
                }
            }

            var headJobPrototypes = GetHeadsPrototypes();
            var headCrewAlive = false;
            foreach (var headPlayer in revaRule.HeadPlayers)
            {
                if (headPlayer.AttachedEntity == null)
                    continue;

                if (TryComp<MobStateComponent>(headPlayer.AttachedEntity, out var stateComp) && stateComp.CurrentState != MobState.Dead
                    && TryComp<MindContainerComponent>(headPlayer.AttachedEntity, out var mindComp) && mindComp.HasMind
                    && mindComp.Mind.CurrentJob?.Prototype != null && headJobPrototypes.Contains(mindComp.Mind?.CurrentJob?.Prototype!))
                {
                    headCrewAlive = true;
                    break;
                }
            }

            if (headRevsAlive && headCrewAlive)
            {
                revaRule.NextTimeCheckConditions = _gameTiming.CurTime + TimeSpan.FromSeconds(30);
                return;
            }

            if (!headRevsAlive)
            {
                revaRule.WinConditions.Add(WinCondition.AllHeadRevsDead);
                SetWinType(uid, WinType.CrewMajor, revaRule);
            }
            else
            {
                revaRule.WinConditions.Add(WinCondition.AllCrewHeadsDead);
                SetWinType(uid, WinType.RevMajor, revaRule);
            }
        }
    }

    private void SetWinType(EntityUid uid, WinType type, RevolutionaryRuleComponent? revaRule)
    {
        if (!Resolve(uid, ref revaRule))
            return;

        revaRule.WinType = type;

        if (type is WinType.CrewMajor or WinType.RevMajor)
            _roundEndSystem.EndRound();
    }

    private void OnPlayersSpawned(RulePlayerJobsAssignedEvent ev)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var revaRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var players = new List<IPlayerSession>(ev.Players);
            var headsPrototypes = GetHeadsPrototypes();
            var nonHeadCrew = players.Where(player =>
            {
                var jobPrototype = player.ContentData()?.Mind?.CurrentJob!.Prototype;
                return jobPrototype != null &&
                       !headsPrototypes.Contains(player.ContentData()?.Mind?.CurrentJob?.Prototype!) &&
                       jobPrototype.CanBeAntag;
            }).ToList();

            var headPlayers = players.Where(player => !nonHeadCrew.Contains(player) &&
                                                      headsPrototypes.Contains(player.ContentData()?.Mind?.CurrentJob?.Prototype!));
            revaRule.HeadPlayers.AddRange(headPlayers);
            foreach (var headPlayer in headPlayers)
            {
                if (headPlayer.AttachedEntity != null)
                    revaRule.ScoreHeadPlayers.Add($"{MetaData(headPlayer.AttachedEntity.Value).EntityName} ({headPlayer.Name})");
            }

            if (nonHeadCrew.Count == 0 || revaRule.HeadPlayers.Count == 0)
            {
                GameTicker.EndGameRule(uid);
                GameTicker.AddGameRule(revaRule.BackupGameRuleProto);
                return;
            }

            var prefList = new List<IPlayerSession>();
            foreach (var player in nonHeadCrew)
            {
                var profile = ev.Profiles[player.UserId];
                if (profile.AntagPreferences.Contains(revaRule.RevaRoleProto))
                {
                    prefList.Add(player);
                }
            }
            if (prefList.Count == 0)
            {
                //_sawmill.Info("Insufficient preferred headrevs, picking at random.");
                prefList = nonHeadCrew;
            }
            var numHeadRevs = MathHelper.Clamp(prefList.Count / revaRule.PlayersPerHeadRev, 1, revaRule.MaxHeadRev);
            var headRevs = new List<IPlayerSession>();

            for (var i = 0; i < numHeadRevs; i++)
            {
                headRevs.Add(_random.PickAndTake(prefList));
                //_sawmill.Info("Selected a preferred Head Rev.");
            }

            foreach (var headRev in headRevs)
            {
                MakeHeadRevolution(headRev, revaRule);
            }

            revaRule.NextTimeCheckConditions = _gameTiming.CurTime + TimeSpan.FromSeconds(180);
        }
    }

    public void MakeHeadRevolution(IPlayerSession headRev, RevolutionaryRuleComponent? revaRule = null)
    {
        if (revaRule == null)
        {
            revaRule = EntityQuery<RevolutionaryRuleComponent>().FirstOrDefault();
            if (revaRule == null)
            {
                GameTicker.StartGameRule(RevolutionaryRuleComponent.RevaGamePresetPrototype, out var ruleEntity);
                revaRule = Comp<RevolutionaryRuleComponent>(ruleEntity);
            }
        }

        var mind = headRev.Data.ContentData()?.Mind;
        if (mind == null)
        {
            //_sawmill.Info("Failed getting mind for picked headrev.");
            return;
        }

        if (mind.OwnedEntity is not { } entity)
        {
            Logger.ErrorS("preset", "Mind picked for headrev did not have an attached entity.");
            return;
        }

        if (mind.OwnedEntity != null)
        {
            var playerEntity = mind.OwnedEntity.Value;
            if (_inventorySystem.TryGetSlotEntity(playerEntity, "back", out var backPack))
            {
                foreach (var itemPrototype in revaRule.StartingItems)
                {
                    var itemEntity = Spawn(itemPrototype, Transform(playerEntity).Coordinates);

                    if (backPack != null)
                    {
                        _storageSystem.Insert(backPack.Value, itemEntity);
                    }
                }
            }
        }

        EntityManager.EnsureComponent<RevolutionaryComponent>(entity);
        revaRule.RevHeadPlayers.Add(headRev);

        _audioSystem.PlayGlobal(revaRule.GreetSound, Filter.Empty().AddPlayer(headRev), false);
        _chatManager.DispatchServerMessage(headRev, Loc.GetString("rev-welcome-headrev"));
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        var query = EntityQueryEnumerator<RevolutionaryRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var revaRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var minPlayers = revaRule.MinPlayers;
            if (!ev.Forced && ev.Players.Length < minPlayers)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("rev-not-enough-ready-players", ("readyPlayersCount", ev.Players.Length), ("minimumPlayers", minPlayers)));
                ev.Cancel();
                return;
            }

            if (ev.Players.Length != 0)
                continue;

            _chatManager.DispatchServerAnnouncement(Loc.GetString("rev-no-one-ready"));
            ev.Cancel();
        }
    }

    private List<JobPrototype> GetHeadsPrototypes()
    {
        return _prototypeManager.EnumeratePrototypes<JobPrototype>().Where(job =>
            job.Access.Contains("Command") || job.AccessGroups.Contains("AllAccess")).ToList();
    }
}
