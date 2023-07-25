using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Chat.Systems;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Mind.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Server.White.Cult.GameRule;
using Content.Server.White.Cult.Runes.Comps;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Components;
using Content.Shared.White.Cult.Runes;
using Content.Shared.White.Cult.UI;
using Content.Shared.White.Mindshield;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly CultRuleSystem _ruleSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Runes
        SubscribeLocalEvent<CultRuneBaseComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<CultRuneOfferingComponent, CultRuneInvokeEvent>(OnInvokeOffering);
        SubscribeLocalEvent<CultRuneBuffComponent, CultRuneInvokeEvent>(OnInvokeBuff);
        SubscribeLocalEvent<CultRuneTeleportComponent, CultRuneInvokeEvent>(OnInvokeTeleport);
        SubscribeLocalEvent<CultRuneApocalypseComponent, CultRuneInvokeEvent>(OnInvokeApocalypse);
        SubscribeLocalEvent<CultRuneReviveComponent, CultRuneInvokeEvent>(OnInvokeRevive);
        SubscribeLocalEvent<CultRuneBarrierComponent, CultRuneInvokeEvent>(OnInvokeBarrier);
        SubscribeLocalEvent<CultRuneSummoningComponent, CultRuneInvokeEvent>(OnInvokeSummoning);
        SubscribeLocalEvent<CultRuneBloodBoilComponent, CultRuneInvokeEvent>(OnInvokeBloodBoil);
        SubscribeLocalEvent<CultistComponent, SummonNarsieDoAfterEvent>(NarsieSpawn);

        SubscribeLocalEvent<CultEmpowerComponent, CultEmpowerSelectedBuiMessage>(OnEmpowerSelected);
        SubscribeLocalEvent<CultEmpowerComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<CultEmpowerComponent, ActivateInWorldEvent>(OnActiveInWorld);

        // UI
        SubscribeLocalEvent<RuneDrawerProviderComponent, UseInHandEvent>(OnRuneDrawerUseInHand);
        SubscribeLocalEvent<RuneDrawerProviderComponent, ListViewItemSelectedMessage>(OnRuneSelected);
        SubscribeLocalEvent<CultTeleportRuneProviderComponent, TeleportRunesListWindowItemSelectedMessage>(OnTeleportRuneSelected);
        SubscribeLocalEvent<CultRuneSummoningProviderComponent, SummonCultistListWindowItemSelectedMessage>(OnCultistSelected);
        SubscribeLocalEvent<RuneDrawerProviderComponent, ComponentInit>(OnInit);

        // Rune drawing/erasing
        SubscribeLocalEvent<CultistComponent, CultDrawEvent>(OnDraw);
        SubscribeLocalEvent<CultistComponent, NameSelectorMessage>(OnChoose);
        SubscribeLocalEvent<CultRuneBaseComponent, InteractUsingEvent>(TryErase);
        SubscribeLocalEvent<CultRuneBaseComponent, CultEraseEvent>(OnErase);

        InitializeBuffSystem();
        InitializeNarsie();
        InitializeSoulShard();
        InitializeConstructs();
        InitializeBarrierSystem();
        InitializeConstructsAbilities();
        InitializeActions();
    }

    private float _timeToDraw;

    private const string TeleportRunePrototypeId = "TeleportRune";
    private const string ApocalypseRunePrototypeId = "ApocalypseRune";
    private const string RitualDaggerPrototypeId = "RitualDagger";
    private const string RunicMetalStackPrototypeId = "CultRunicMetal20";
    private const string RunicMetalPrototypeId = "CultRunicMetal";
    private const string SteelPrototypeId = "Steel";
    private const string NarsiePrototypeId = "Narsie";
    private const string CultBarrierPrototypeId = "CultBarrier";

    private bool _narsieSummoned;
    private bool _doAfterAlreadyStarted;

    private IPlayingAudioStream? _playingStream;

    private readonly SoundPathSpecifier _teleportInSound = new("/Audio/White/Cult/veilin.ogg");
    private readonly SoundPathSpecifier _teleportOutSound = new("/Audio/White/Cult/veilout.ogg");

    private readonly SoundPathSpecifier _magic = new("/Audio/White/Cult/magic.ogg");

    private readonly SoundPathSpecifier _apocRuneStartDrawing = new("/Audio/White/Cult/startdraw.ogg");
    private readonly SoundPathSpecifier _apocRuneEndDrawing = new("/Audio/White/Cult/finisheddraw.ogg");
    private readonly SoundPathSpecifier _narsie40Sec = new("/Audio/White/Cult/40sec.ogg");

    /*
    * Rune draw start ----
     */

    private void OnInit(EntityUid uid, RuneDrawerProviderComponent component, ComponentInit args)
    {
        if(component.UserInterface == null)
            return;

        _ui.SetUiState(component.UserInterface, new ListViewBUIState(component.RunePrototypes, false));
    }

    private void OnRuneDrawerUseInHand(EntityUid uid, RuneDrawerProviderComponent component, UseInHandEvent args)
    {
        if(component.UserInterface == null)
            return;

        if(!TryComp<ActorComponent>(args.User, out var actorComponent))
            return;

        if (!HasComp<CultistComponent>(args.User))
            return;

        _ui.ToggleUi(component.UserInterface, actorComponent.PlayerSession);
    }

    private void OnRuneSelected(EntityUid uid, RuneDrawerProviderComponent component, ListViewItemSelectedMessage args)
    {
        if (args.Session.AttachedEntity == null)
            return;

        var runePrototype = args.SelectedItem;
        var whoCalled = args.Session.AttachedEntity.Value;

        if(!TryComp<ActorComponent>(whoCalled, out var actorComponent))
            return;

        if (!TryDraw(whoCalled, runePrototype))
            return;

        if (component.UserInterface != null)
            _ui.CloseUi(component.UserInterface, actorComponent.PlayerSession);
    }

    private bool TryDraw(EntityUid whoCalled, string runePrototype)
    {
        _timeToDraw = 4f;

        if (HasComp<CultBuffComponent>(whoCalled))
            _timeToDraw /= 2;

        if (runePrototype == ApocalypseRunePrototypeId)
        {
            _timeToDraw = 120.0f;
            _chat.DispatchGlobalAnnouncement("Cult started to draw ritual rune!", "CULT", true, _apocRuneStartDrawing, colorOverride: Color.DarkRed);
        }

        if (!IsAllowedToDraw(whoCalled))
            return false;

        var ev = new CultDrawEvent
        {
            Rune = runePrototype
        };

        var argsDoAfterEvent = new DoAfterArgs(whoCalled, _timeToDraw, ev, whoCalled)
        {
            BreakOnUserMove = true,
            NeedHand = true
        };

        if (!_doAfterSystem.TryStartDoAfter(argsDoAfterEvent))
            return false;

        _audio.PlayPvs("/Audio/White/Cult/butcher.ogg", whoCalled, AudioParams.Default.WithMaxDistance(2f));
        return true;
    }

    private void OnDraw(EntityUid uid, CultistComponent comp, CultDrawEvent args)
    {
        if (args.Cancelled)
            return;

        var howMuchBloodTake = -10;
        var rune = args.Rune;
        var user = args.User;

        if (HasComp<CultBuffComponent>(user))
            howMuchBloodTake /= 2;

        if (!TryComp<BloodstreamComponent>(user, out var bloodstreamComponent))
            return;

        _bloodstreamSystem.TryModifyBloodLevel(user, howMuchBloodTake, bloodstreamComponent);
        _audio.PlayPvs("/Audio/White/Cult/blood.ogg", user, AudioParams.Default.WithMaxDistance(2f));

        if (rune == TeleportRunePrototypeId)
        {
            if (!TryComp<ActorComponent>(user, out var actorComponent))
                return;

            _ui.GetUiOrNull(user, NameSelectorUIKey.Key)?.Open(actorComponent.PlayerSession);
            return;
        }

        SpawnRune(user, rune);
    }

    private void OnChoose(EntityUid uid, CultistComponent component, NameSelectorMessage args)
    {
        if (!TryComp<ActorComponent>(uid, out var actorComponent))
            return;

        _ui.GetUiOrNull(uid, NameSelectorUIKey.Key)?.Close(actorComponent.PlayerSession);

        SpawnRune(uid, TeleportRunePrototypeId, true, args.Name);
    }

    //Erasing start

    private void TryErase(EntityUid uid, CultRuneBaseComponent component, InteractUsingEvent args)
    {
        var entityPrototype = _entityManager.GetComponent<MetaDataComponent>(args.Used).EntityPrototype;

        if (entityPrototype != null)
        {
            var used = entityPrototype.ID;
            var user = args.User;
            var target = args.Target;
            var time = 3;

            if (used != RitualDaggerPrototypeId)
                return;

            if (!HasComp<CultistComponent>(user))
                return;

            if (HasComp<CultBuffComponent>(user))
                time /= 2;

            var ev = new CultEraseEvent
            {
                TargetEntityId = target
            };

            var argsDoAfterEvent = new DoAfterArgs(user, time, ev, target)
            {
                BreakOnUserMove = true,
                NeedHand = true
            };

            if (_doAfterSystem.TryStartDoAfter(argsDoAfterEvent))
            {
                _popupSystem.PopupEntity("You started erasing rune", target);
            }
        }
    }

    private void OnErase(EntityUid uid, CultRuneBaseComponent component, CultEraseEvent args)
    {
        if (args.Cancelled)
            return;

        _entityManager.DeleteEntity(args.TargetEntityId);
        _popupSystem.PopupEntity("You erased rune.", args.User);
    }

    //Erasing end

    /*
    * Rune draw end ----
     */

    //------------------------------------------//

    /*
     * Base Start ----
     */

    private void OnActivate(EntityUid uid, CultRuneBaseComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!HasComp<CultistComponent>(args.User))
            return;

        var cultists = new HashSet<EntityUid>
        {
            args.User
        };

        if (component.InvokersMinCount > 1 || component.GatherInvokers)
            cultists = GatherCultists(uid, component.CultistGatheringRange);

        if (cultists.Count < component.InvokersMinCount)
        {
            _popupSystem.PopupEntity("Not enough cultists.", args.User, args.User);
            return;
        }

        var ev = new CultRuneInvokeEvent(uid, args.User, cultists);
        RaiseLocalEvent(uid, ev);

        if (ev.Result)
        {
            OnAfterInvoke(uid, args.User, cultists);
        }
    }

    private void OnAfterInvoke(EntityUid rune, EntityUid user, HashSet<EntityUid> cultists)
    {
        if (!_entityManager.TryGetComponent<CultRuneBaseComponent>(rune, out var component))
            return;

        foreach (var cultist in cultists)
        {
            _chat.TrySendInGameICMessage(cultist, component.InvokePhrase, InGameICChatType.Speak, false, false, null, null, null, false);
        }
    }

    /*
    * Base End ----
    */


    //------------------------------------------//


    /*
     * Offering Rune START ----
     */

    private void OnInvokeOffering(EntityUid uid, CultRuneOfferingComponent component, CultRuneInvokeEvent args)
    {
        var targets = _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        targets.RemoveWhere(x => !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) || HasComp<CultistComponent>(x));

        if (targets.Count == 0)
            return;

        var victim = FindNearestTarget(uid, targets.ToList());

        if (victim == null)
            return;

        _entityManager.TryGetComponent<MobStateComponent>(victim.Value, out var state);

         if (state == null)
             return;

         bool result;

         if (state.CurrentState != MobState.Dead)
         {
             var canBeConverted = _entityManager.TryGetComponent<MindContainerComponent>(victim.Value, out var mind) && mind.HasMind;

             result = canBeConverted && !_entityManager.TryGetComponent<MindShieldComponent>(victim.Value, out _)
                 ? Convert(uid, victim.Value, args.User, args.Cultists)
                 : Sacrifice(uid, victim.Value, args.User, args.Cultists);
         }
         else
         {
             result = SacrificeNonObjectiveDead(uid, victim.Value, args.User, args.Cultists);
         }

         args.Result = result;
    }

    private bool Sacrifice(EntityUid rune,EntityUid target, EntityUid user, HashSet<EntityUid> cultists)
    {
        if (!_entityManager.TryGetComponent<CultRuneOfferingComponent>(rune, out var offering))
            return false;

        if (cultists.Count < offering.SacrificeMinCount)
        {
            _popupSystem.PopupEntity("Can`t be converted, not enough cultists to sacriface", user, user);
            return false;
        }

        if (!SpawnShard(target))
        {
            _bodySystem.GibBody(target);
            AddUsesToRevive();
        }
        else
        {
            AddUsesToRevive();
        }

        return true;
    }

    private bool SacrificeNonObjectiveDead(EntityUid rune, EntityUid target, EntityUid user, HashSet<EntityUid> cultists)
    {
        if (!_entityManager.TryGetComponent<CultRuneOfferingComponent>(rune, out var offering))
            return false;

        if (cultists.Count < offering.SacrificeDeadMinCount)
        {
            _popupSystem.PopupEntity("Not enough cultists to sacriface!", user, user);
            return false;
        }

        if (!SpawnShard(target))
        {
            _bodySystem.GibBody(target);
            AddUsesToRevive();
        }
        else
        {
            AddUsesToRevive();
        }

        return true;
    }

    private bool Convert(EntityUid rune, EntityUid target, EntityUid user, HashSet<EntityUid> cultists)
    {
        if (!_entityManager.TryGetComponent<CultRuneOfferingComponent>(rune, out var offering))
            return false;

        if (cultists.Count < offering.ConvertMinCount)
        {
            _popupSystem.PopupEntity("Not enough cultists to convert! Needed minimum 2!", user, user);
            return false;
        }

        EnsureComp<CultistComponent>(target);
        SpawnCultistItems(target);
        HealCultist(target);

        return true;
    }

    /*
     * Offering Rune END ----
     */

    //------------------------------------------//

    /*
    * Buff Rune Start ----
     */

    private void OnInvokeBuff(EntityUid uid, CultRuneBuffComponent component, CultRuneInvokeEvent args)
    {
        var targets = _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        targets.RemoveWhere(x => !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) || !_entityManager.HasComponent<CultistComponent>(x));

        if (targets.Count == 0)
            return;

        var victim = FindNearestTarget(uid, targets.ToList());

        if (victim == null)
            return;

        _entityManager.TryGetComponent<MobStateComponent>(victim.Value, out var state);

        var result = false;


        if (state != null && state.CurrentState != MobState.Dead)
        {
            result = AddCultistBuff(victim.Value, args.User);
        }

        args.Result = result;
    }

    private bool AddCultistBuff(EntityUid target, EntityUid user)
    {
        if (HasComp<CultBuffComponent>(target))
        {
            _popupSystem.PopupEntity("He arleady buffed!", user, user);
            return false;
        }

        EnsureComp<CultBuffComponent>(target);
        return true;
    }

    /*
    * Empower Rune End ----
     */

    //------------------------------------------//


    /*
    * Teleport rune start ----
     */

    private void OnInvokeTeleport(EntityUid uid, CultRuneTeleportComponent component, CultRuneInvokeEvent args)
    {
        var targets = _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        if (targets.Count == 0)
            return;

        if (targets.Count > 1)
        {
            args.Result = Teleport(uid, args.User, victims: targets.ToList());
        }
        else if (targets.Count == 1)
        {
            var victim = FindNearestTarget(uid, targets.ToList());

            if (victim == null)
                return;

            args.Result = Teleport(uid, args.User, victim.Value);
        }
    }

    private bool Teleport(EntityUid rune, EntityUid user, EntityUid? victim = null, List<EntityUid>? victims = null)
    {
        var runes = EntityQuery<CultRuneTeleportComponent>();
        var list = new List<int>();
        var labels = new List<string>();

        foreach (var teleportRune in runes)
        {
            if (!TryComp<CultRuneTeleportComponent>(teleportRune.Owner, out var teleportComponent))
                continue;

            if (teleportComponent.Label == null)
                continue;

            if (teleportRune.Owner == rune)
                continue;

            if (!int.TryParse(teleportRune.Owner.ToString(), out var intValue))
                continue;

            list.Add(intValue);
            labels.Add(teleportComponent.Label);
        }

        if (!TryComp<ActorComponent>(user, out var actorComponent))
            return false;

        var ui = _ui.GetUiOrNull(user, RuneTeleporterUiKey.Key);

        if (ui == null)
            return false;

        if (list.Count == 0)
        {
            _popupSystem.PopupEntity("Teleport runes not found.", user, user);
            return false;
        }

        _entityManager.EnsureComponent<CultTeleportRuneProviderComponent>(user, out var providerComponent);
        providerComponent.Target = victim;
        providerComponent.Targets = victims;
        providerComponent.BaseRune = rune;

        _ui.SetUiState(ui, new TeleportRunesListWindowBUIState(list, labels));

        if (_ui.IsUiOpen(user, ui.UiKey))
            return false;

        _ui.ToggleUi(ui, actorComponent.PlayerSession);
            return true;
    }

    private void OnTeleportRuneSelected(EntityUid uid, CultTeleportRuneProviderComponent component, TeleportRunesListWindowItemSelectedMessage args)
    {
        var target = component.Target;
        var moreTargets = component.Targets;
        var user = args.Session.AttachedEntity;
        var selectedRune = new EntityUid(args.SelectedItem);
        var baseRune = component.BaseRune;

        if (user == null || baseRune == null)
            return;

        if (!TryComp<TransformComponent>(selectedRune, out var xFormSelected) ||
            !TryComp<TransformComponent>(baseRune, out var xFormBase))
            return;

        if (moreTargets != null)
        {
            foreach (var targets in moreTargets)
            {
                _xform.SetCoordinates(targets, xFormSelected.Coordinates);
            }
        }

        if (target != null)
        {
            _xform.SetCoordinates(target.Value, xFormSelected.Coordinates);
        }

        //Play tp sound
        _audio.PlayPvs(_teleportInSound, xFormSelected.Coordinates);
        _audio.PlayPvs(_teleportOutSound, xFormBase.Coordinates);

        if (HasComp<CultTeleportRuneProviderComponent>(user.Value))
        {
            RemComp<CultTeleportRuneProviderComponent>(user.Value);
        }
    }

    /*
    * Teleport rune end ----
     */


    //------------------------------------------//


    /*
    * Apocalypse rune start ----
     */

    private void OnInvokeApocalypse(EntityUid uid, CultRuneApocalypseComponent component, CultRuneInvokeEvent args)
    {
        var targets = _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        targets.RemoveWhere(x => !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) | !_entityManager.HasComponent<CultistComponent>(x));

        args.Result = TrySummonNarsie(args.User, args.Cultists, component);
    }

    private bool TrySummonNarsie(EntityUid user, HashSet<EntityUid> cultists, CultRuneApocalypseComponent component)
    {
        var canSummon = _ruleSystem.CanSummonNarsie();

        if (!canSummon)
        {
            _popupSystem.PopupEntity(Loc.GetString("Do all your task! Need minimum 10 cultists in cult!"), user, user);
            return false;
        }

        if (cultists.Count < component.SummonMinCount)
        {
            _popupSystem.PopupEntity("Need minimum 10 cultists!", user, user);
            return false;
        }

        if (_narsieSummoned)
        {
            _popupSystem.PopupEntity("You arleady summoned me!", user, user);
            return false;
        }

        if (_doAfterAlreadyStarted)
        {
            _popupSystem.PopupEntity("Someone arleady summoning narsie!", user, user);
            return false;
        }

        if (!TryComp<DoAfterComponent>(user, out var doAfterComponent))
        {
            if (doAfterComponent is { AwaitedDoAfters.Count: >= 1 })
            {
                _popupSystem.PopupEntity("You arleady doing something!", user, user);
                return false;
            }
        }

        var ev = new SummonNarsieDoAfterEvent();

        var argsDoAfterEvent = new DoAfterArgs(user, TimeSpan.FromSeconds(40), ev, user)
        {
            BreakOnUserMove = true
        };

        if (!_doAfterSystem.TryStartDoAfter(argsDoAfterEvent))
            return false;

        _popupSystem.PopupEntity(Loc.GetString("You need to stay still!"), user, user, PopupType.LargeCaution);

        _doAfterAlreadyStarted = true;

        _chat.DispatchGlobalAnnouncement("Cultists, started ritual. You have 40 sec to prevent this!", "CULT", false, colorOverride: Color.DarkRed);
        _playingStream = _audio.PlayGlobal(_narsie40Sec, Filter.Broadcast(), false, AudioParams.Default.WithLoop(true));

        return true;
    }

    private void NarsieSpawn(EntityUid uid, CultistComponent component, SummonNarsieDoAfterEvent args)
    {
        if (_playingStream != null)
            _playingStream.Stop();

        _doAfterAlreadyStarted = false;

        if (args.Cancelled)
        {
            _chat.DispatchGlobalAnnouncement("Someone prevent ritual..", "CULT", false, colorOverride: Color.DarkRed);
            return;
        }

        var transform = CompOrNull<TransformComponent>(args.User)?.Coordinates;
        if (transform == null)
            return;

        _entityManager.SpawnEntity(NarsiePrototypeId, transform.Value);
        _narsieSummoned = true;

        _chat.DispatchGlobalAnnouncement("NARSIE HAS RISEN", "CULT", true, _apocRuneEndDrawing, colorOverride: Color.DarkRed);

        var ev = new CultNarsieSummoned();
        RaiseLocalEvent(ev);
    }


    /*
    * Apocalypse rune end ----
     */

    //------------------------------------------//

     /*
    * Revive rune start ----
        */

    private void OnInvokeRevive(EntityUid uid, CultRuneReviveComponent component, CultRuneInvokeEvent args)
    {
        var targets = _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        targets.RemoveWhere(x => !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) || !HasComp<CultistComponent>(x));

        if (targets.Count == 0)
            return;

        var victim = FindNearestTarget(uid, targets.ToList());

        if (victim == null)
            return;

        _entityManager.TryGetComponent<MobStateComponent>(victim.Value, out var state);

        if (state == null)
            return;

        if (state.CurrentState != MobState.Dead && state.CurrentState != MobState.Critical)
        {
            _popupSystem.PopupEntity("He arleady alive!", args.User, args.User);
            return;
        }

        var result = Revive(victim.Value, args.User, component);

        args.Result = result;
    }

    private bool Revive(EntityUid target, EntityUid user, CultRuneReviveComponent component)
    {
        if (component.UsesToRevive < component.UsesHave)
        {
            _popupSystem.PopupEntity("No charges", user, user);
            return false;
        }

        component.UsesHave-= 3;

        _entityManager.EventBus.RaiseLocalEvent(target, new RejuvenateEvent());
            return true;
    }

        /*
    * Revive rune end ----
        */

    //------------------------------------------//

    /*
    * Barrier rune start ----
     */

    private void OnInvokeBarrier(EntityUid uid, CultRuneBarrierComponent component, CultRuneInvokeEvent args)
    {
        args.Result = SpawnBarrier(args.Rune);
    }

    private bool SpawnBarrier(EntityUid rune)
    {
        var transform = CompOrNull<TransformComponent>(rune)?.Coordinates;

        if (transform == null)
            return false;

        _entityManager.SpawnEntity(CultBarrierPrototypeId, transform.Value);
        _entityManager.DeleteEntity(rune);

        return true;
    }

    /*
    * Barrier rune end ----
    */

    //------------------------------------------//

    /*
   * Summoning rune start ----
    */

    private void OnInvokeSummoning(EntityUid uid, CultRuneSummoningComponent component, CultRuneInvokeEvent args)
    {
        args.Result = Summon(uid, args.User, args.Cultists, component);
    }

    private bool Summon(EntityUid rune, EntityUid user, HashSet<EntityUid> cultistHashSet, CultRuneSummoningComponent component)
    {
        var cultists = EntityQuery<CultistComponent>();
        var list = new List<int>();
        var labels = new List<string>();

        if (cultistHashSet.Count < component.SummonMinCount)
        {
            _popupSystem.PopupEntity("Needed minimum 2 cultist.", user, user);
            return false;
        }

        foreach (var cultist in cultists)
        {
            if (!TryComp<MetaDataComponent>(cultist.Owner, out var meta))
                continue;

            if (cultistHashSet.Contains(cultist.Owner))
                continue;

            if (!int.TryParse(cultist.Owner.ToString(), out var intValue))
                continue;

            list.Add(intValue);
            labels.Add(meta.EntityName);
        }

        if (!TryComp<ActorComponent>(user, out var actorComponent))
            return false;

        var ui = _ui.GetUiOrNull(user, SummonCultistUiKey.Key);

        if (ui == null)
            return false;

        if (list.Count == 0)
        {
            _popupSystem.PopupEntity("Cultists not found.", user, user);
            return false;
        }

        _entityManager.EnsureComponent<CultRuneSummoningProviderComponent>(user, out var providerComponent);
        providerComponent.BaseRune = rune;

        _ui.SetUiState(ui, new SummonCultistListWindowBUIState(list, labels));

        if (_ui.IsUiOpen(user, ui.UiKey))
            return false;

        _ui.ToggleUi(ui, actorComponent.PlayerSession);
            return true;
    }

    private void OnCultistSelected(EntityUid uid, CultRuneSummoningProviderComponent component, SummonCultistListWindowItemSelectedMessage args)
    {
        var user = args.Session.AttachedEntity;
        var target = new EntityUid(args.SelectedItem);
        var baseRune = component.BaseRune;

        if (user == null || baseRune == null)
            return;

        if (!TryComp<TransformComponent>(baseRune, out var xFormBase))
            return;

        _xform.SetCoordinates(target, xFormBase.Coordinates);

        _audio.PlayPvs(_teleportInSound, xFormBase.Coordinates);

        if (HasComp<CultRuneSummoningProviderComponent>(user.Value))
        {
            RemComp<CultRuneSummoningProviderComponent>(user.Value);
        }
    }

    /*
   * Summoning rune end ----
    */

    //------------------------------------------//


    /*
   * BloodBoil rune start ----
    */

    private void OnInvokeBloodBoil(EntityUid uid, CultRuneBloodBoilComponent component, CultRuneInvokeEvent args)
    {
        args.Result = PrepareShoot(uid, args.User, args.Cultists, 1.0f, component);
    }

    private bool PrepareShoot(EntityUid rune, EntityUid user, HashSet<EntityUid> cultists, float severity, CultRuneBloodBoilComponent component)
    {
        if (cultists.Count < component.SummonMinCount)
        {
            _popupSystem.PopupEntity("Needed minimum 3 cultists", user, user);
            return false;
        }

        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(rune);

        foreach (var cultist in cultists)
        {
            if (!TryComp<BloodstreamComponent>(cultist, out var bloodstreamComponent))
                return false;

            _bloodstreamSystem.TryModifyBloodLevel(cultist, -40, bloodstreamComponent);
        }

        var projectileCount = (int)MathF.Round(MathHelper.Lerp(component.MinProjectiles, component.MaxProjectiles, severity));
        var inRange = _lookup.GetEntitiesInRange(rune, component.ProjectileRange * severity, LookupFlags.Dynamic);
        inRange.RemoveWhere(x => !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) || _entityManager.HasComponent<CultistComponent>(x));
        var list = inRange.ToList();

        if (list.Count == 0)
        {
            _popupSystem.PopupEntity("No targets!", user, user);
            return false;
        }

        _random.Shuffle(list);

        while (projectileCount > 0)
        {
            var target = _random.Pick(list);
            var targetCoords = xformQuery.GetComponent(target).Coordinates.Offset(_random.NextVector2(0.5f));
            var flammable = GetEntityQuery<FlammableComponent>();

            if (!flammable.TryGetComponent(target, out var fl))
                continue;

            fl.FireStacks += _random.Next(1, 3);

            _flammableSystem.Ignite(target, fl);

            Shoot(
                rune,
                component,
                xform.Coordinates,
                targetCoords,
                severity);
            projectileCount--;
        }

        _audio.PlayPvs(_magic, rune, AudioParams.Default.WithMaxDistance(2f));

        return true;
    }

    private void Shoot(
        EntityUid uid,
        CultRuneBloodBoilComponent component,
        EntityCoordinates coords,
        EntityCoordinates targetCoords,
        float severity)
    {
        var mapPos = coords.ToMap(EntityManager, _xform);

        var spawnCoords = _mapManager.TryFindGridAt(mapPos, out var gridUid, out _)
            ? coords.WithEntityId(gridUid, EntityManager)
            : new(_mapManager.GetMapEntityId(mapPos.MapId), mapPos.Position);

        var ent = Spawn(component.ProjectilePrototype, spawnCoords);
        var direction = targetCoords.ToMapPos(EntityManager, _xform) - mapPos.Position;

        if (!TryComp<ProjectileComponent>(ent, out var comp))
            return;

        comp.Damage *= severity;

        _gunSystem.ShootProjectile(ent, direction, Vector2.Zero, uid, uid, component.ProjectileSpeed);
    }

    /*
   * BloodBoil rune end ----
    */


    //------------------------------------------//

    /*
    * Empower rune start ----
    */

    private void OnActiveInWorld(EntityUid uid, CultEmpowerComponent component, ActivateInWorldEvent args)
    {
        if(!component.IsRune || !TryComp<CultistComponent>(args.User, out var cultist) || !TryComp<ActorComponent>(args.User, out var actor)) return;

        _ui.TryOpen(uid, CultEmpowerUiKey.Key, actor.PlayerSession);
    }

    private void OnUseInHand(EntityUid uid, CultEmpowerComponent component, UseInHandEvent args)
    {
        if(!TryComp<CultistComponent>(args.User, out var cultist) || !TryComp<ActorComponent>(args.User, out var actor)) return;

        _ui.TryOpen(uid, CultEmpowerUiKey.Key, actor.PlayerSession);
    }

    private void OnEmpowerSelected(EntityUid uid, CultEmpowerComponent component, CultEmpowerSelectedBuiMessage args)
    {
        var playerEntity = args.Session.AttachedEntity;

        if(!playerEntity.HasValue || !TryComp<CultistComponent>(playerEntity, out _) || !TryComp<ActionsComponent>(playerEntity, out var actionsComponent))
            return;

        var cultistsActions = actionsComponent.Actions.Intersect(CultistComponent.CultistActions).Count();

        var action = CultistComponent.CultistActions.FirstOrDefault(x => x.Equals(args.ActionType));

        if(action == null)
            return;

        if (component.IsRune)
        {
            if (cultistsActions > component.MaxAllowedCultistActions)
            {
                _popupSystem.PopupEntity(Loc.GetString("cult-too-much-empowers"), uid);
                return;
            }

            _actionsSystem.AddAction(playerEntity.Value, action, null!);
        }
        else if(cultistsActions < component.MinRequiredCultistActions)
        {
            _actionsSystem.AddAction(playerEntity.Value, action, null!);
        }
    }

    /*
    * Empower rune end ----
    */

    //------------------------------------------//


    /*
    * Helpers Start ----
     */

    private EntityUid? FindNearestTarget(EntityUid uid, List<EntityUid> targets)
    {
        if (!_entityManager.TryGetComponent<TransformComponent>(uid, out var runeTransform))
            return null;

        var range = 999f;
        EntityUid? victim = null;

        foreach (var target in targets)
        {
            if (!_entityManager.TryGetComponent<TransformComponent>(target, out var targetTransform))
                continue;

            runeTransform.Coordinates.TryDistance(_entityManager, targetTransform.Coordinates, out var newRange);

            if (newRange < range)
            {
                range = newRange;
                victim = target;
            }
        }

        return victim;
    }


    private HashSet<EntityUid> GatherCultists(EntityUid uid, float range)
    {
        var entities = _lookup.GetEntitiesInRange(uid,range, LookupFlags.Dynamic);
        entities.RemoveWhere(x => !HasComp<CultistComponent>(x));

        return entities;
    }

    private void SpawnRune(EntityUid uid, string? rune, bool teleportRune = false, string? label = null)
    {
        var transform = CompOrNull<TransformComponent>(uid)?.Coordinates;

        if (transform == null)
            return;

        if (rune == null)
            return;

        if (teleportRune)
        {
            var teleportRuneEntity = _entityManager.SpawnEntity(rune, transform.Value);

            _entityManager.TryGetComponent<CultRuneTeleportComponent>(teleportRuneEntity, out var sex);
            {
                if (sex == null)
                    return;

                label = string.IsNullOrEmpty(label) ? "безымянная метка" : label;

                if (label.Length > 18)
                {
                    label = label.Substring(0, 18);
                }

                sex.Label = label;
            }

            return;
        }

        if (rune == ApocalypseRunePrototypeId)
        {
            if (!_entityManager.TryGetComponent(uid, out TransformComponent? transComp))
            {
                return;
            }

            var pos =  transComp.MapPosition;
            var x = (int) pos.X;
            var y = (int) pos.Y;
            var posText = $"(x = {x}, y = {y})";
            _chat.DispatchGlobalAnnouncement($"Культ закончил рисовать руну ритуала разрыва измерений! Координаты: {posText}", "CULT", true, _apocRuneEndDrawing, colorOverride: Color.DarkRed);
        }

        _entityManager.SpawnEntity(rune, transform.Value);
    }

    private bool SpawnShard(EntityUid target)
    {
        if (!_entityManager.TryGetComponent<MindContainerComponent>(target, out var mindComponent))
            return false;

        var transform = CompOrNull<TransformComponent>(target)?.Coordinates;

        if (transform == null)
            return false;

        if (mindComponent.Mind == null)
            return false;

        var shard = _entityManager.SpawnEntity("SoulShard", transform.Value);

        _mindSystem.TransferTo(mindComponent.Mind, shard);

        _bodySystem.GibBody(target);

        return true;
    }

    private void AddUsesToRevive()
    {
        var runes = EntityQuery<CultRuneReviveComponent>();

        foreach (var rune in runes)
        {
            rune.UsesHave += 1;
        }
    }

    private bool IsAllowedToDraw(EntityUid uid)
    {
        var transform = Transform(uid);
        var gridUid = transform.GridUid;
        var tile = transform.Coordinates.GetTileRef();

        if (!gridUid.HasValue)
        {
            _popupSystem.PopupEntity("Нельзя рисовать в космосе.", uid, uid);
            return false;
        }

        if (!tile.HasValue)
        {
            _popupSystem.PopupEntity("Нельзя рисовать в космосе.", uid, uid);
            return false;
        }

        return true;
    }

    private void SpawnCultistItems(EntityUid yeban)
    {
        var transform = CompOrNull<TransformComponent>(yeban)?.Coordinates;

        if (transform == null)
            return;

        var dagger = _entityManager.SpawnEntity(RitualDaggerPrototypeId, transform.Value);
        var ruinicsexoooo = _entityManager.SpawnEntity(RunicMetalStackPrototypeId, transform.Value);

        _handsSystem.TryPickupAnyHand(yeban, dagger);
        _handsSystem.TryPickupAnyHand(yeban, ruinicsexoooo);
    }

    private void HealCultist(EntityUid player)
    {
        var damageSpecifier = _prototypeManager.Index<DamageGroupPrototype>("Brute");
        var damageSpecifier2 = _prototypeManager.Index<DamageGroupPrototype>("Burn");

        _damageableSystem.TryChangeDamage(player, new DamageSpecifier(damageSpecifier, -40));
        _damageableSystem.TryChangeDamage(player, new DamageSpecifier(damageSpecifier2, -40));
    }

    /*
    * Helpers End ----
     */
}

