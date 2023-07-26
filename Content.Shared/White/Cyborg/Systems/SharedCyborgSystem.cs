using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Content.Shared.Wires;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.White.Cyborg.Systems;

public abstract class SharedCyborgSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CyborgComponent, ToolUserAttemptUseEvent>(OnToolUseAttempt);
        SubscribeLocalEvent<CyborgComponent, ModuleRemovalFinishedEvent>(OnModuleRemoval);
        SubscribeLocalEvent<CyborgComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CyborgComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<CyborgComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<CyborgComponent, ComponentGetState>(OnComponentGetState);
        SubscribeLocalEvent<CyborgComponent, ComponentHandleState>(OnComponentHandleState);
    }

    private void OnComponentStartup(EntityUid uid, CyborgComponent component, ComponentStartup args)
    {
        component.ModuleContainer = _container.EnsureContainer<Container>(uid, CyborgComponent.ModuleContainerName);
        component.BatterySlot = _container.EnsureContainer<ContainerSlot>(uid, CyborgComponent.BatterySlotId);
        component.InstrumentContainer =
            _container.EnsureContainer<Container>(uid, CyborgComponent.InstrumentContainerName);
    }

    private void OnMapInit(EntityUid uid, CyborgComponent component, MapInitEvent args)
    {
        InitModules(uid, component);

        if (component.BatterySlot.ContainedEntity.HasValue)
        {
            var ev = new BatteryInsertedEvent(uid, component.BatterySlot.ContainedEntity.Value);
            RaiseLocalEvent(uid, ev);
        }

        _actionBlocker.UpdateCanMove(uid);
        Dirty(component);
    }


    private void OnExamined(EntityUid uid, CyborgComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!component.Active)
            args.PushMarkup(Loc.GetString("cyborg-not-active"));

        if (component.ModuleContainer.ContainedEntities.Count == 0)
            args.PushMarkup(Loc.GetString("modules-no-modules"));
        else
        {
            args.PushMarkup(Loc.GetString("examine-modules-prefix"));
            foreach (var entity in component.ModuleContainer.ContainedEntities)
            {
                if (TryComp<CyborgModuleComponent>(entity, out var cyborgModuleComponent))
                    args.PushMarkup(" - " + cyborgModuleComponent.Name);
            }
        }

        args.PushMarkup(Loc.GetString("examine-borg-energy", ("energy", component.Energy),
            ("maxEnergy", component.MaxEnergy)));
    }


    private void OnModuleRemoval(EntityUid uid, CyborgComponent component, ModuleRemovalFinishedEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;
        var contained = component.ModuleContainer.ContainedEntities.ToArray();
        if (contained.Length > 0)
        {
            foreach (var ent in contained)
            {
                _hands.PickupOrDrop(args.User, ent);
                if (!TryComp<CyborgModuleComponent>(ent, out var cyborgModuleComponent))
                    continue;
                var ev = new ModuleRemoveEvent(ent, uid);
                RaiseLocalEvent(ent, ev);
            }
        }

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("modules-all-extracted"), uid, args.User);


        _audio.PlayPredicted(component.ModuleExtractionSound, uid, args.User);
    }


    private void OnToolUseAttempt(EntityUid uid, CyborgComponent component, ref ToolUserAttemptUseEvent args)
    {
        if (args.Target == uid)
            args.Cancelled = true;
    }

    private void OnInteractUsing(EntityUid uid, CyborgComponent component, InteractUsingEvent args)
    {
        if (!TryComp<WiresPanelComponent>(uid, out var panel) || TryComp<CyborgComponent>(args.User, out _))
            return;
        if (!panel.Open || args.Handled)
            return;

        var toolEvent = new ToolUseOnBorgEvent(uid, args.Used, args.User);
        RaiseLocalEvent(args.Used, toolEvent);

        args.Handled = toolEvent.Handled;
    }

    public void ChangeConsumption(EntityUid uid, FixedPoint2 volume, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component) || component.Consumption + volume <= 0)
            return;

        component.Consumption += volume;
    }


    public bool TogglePanelLock(EntityUid uid, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        return component.PanelLocked = !component.PanelLocked;
    }


    public void InitModules(EntityUid uid, CyborgComponent component)
    {
        if (!component.Initialized || _net.IsClient)
            return;

        //What is already in the borg we initialize
        foreach (var entityUid in component.ModuleContainer.ContainedEntities)
        {
            if (!TryComp<CyborgModuleComponent>(entityUid, out var cyborgModuleComponent))
                continue;
            component.ModuleUids.Add(entityUid);
            var ev = new ModuleInsertEvent(entityUid, uid);
            RaiseLocalEvent(entityUid, ev);
        }

        Dirty(component);
    }


    /// <summary>
    ///     checks if the list has the same accesses as the borg
    /// </summary>
    /// <param name="cyborgUid"></param>
    /// <param name="access"></param>
    /// <param name="component"></param>
    /// <seealso cref="CyborgComponent.UnlockAccessTags" />
    /// <returns></returns>
    public bool HasAccess(EntityUid cyborgUid, HashSet<string> access, CyborgComponent? component = null)
    {
        if (!Resolve(cyborgUid, ref component))
            return false;

        foreach (var a in component.UnlockAccessTags)
        {
            if (access.Contains(a))
                return true;
        }

        return false;
    }


    public bool TryInsertModule(EntityUid uid, EntityUid used, EntityUid user, CyborgComponent? component = null,
        CyborgModuleComponent? cyborgModuleComponent = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(used, ref cyborgModuleComponent))
            return false;

        if (component.ModuleSlots <= component.ModuleContainer.ContainedEntities.Count)
        {
            if (_net.IsClient && _timing.IsFirstTimePredicted)
                _popup.PopupEntity(Loc.GetString("modules-slots-already-full"), uid, user);
            return false;
        }

        if (component.ModuleContainer.Insert(used))
        {
            if (_net.IsClient && _timing.IsFirstTimePredicted)
                _popup.PopupEntity(Loc.GetString("modules-successfully-installed"), uid, user);
            _audio.PlayPredicted(component.ModuleInsertionSound, uid, user);

            var ev = new ModuleInsertEvent(used, uid);
            RaiseLocalEvent(used, ev);
            return true;
        }

        return false;
    }


    public bool TryRemoveModule(EntityUid uid, EntityUid used, EntityUid user, CyborgComponent component,
        ToolComponent tool)
    {
        if (component.ModuleContainer.ContainedEntities.Count == 0)
        {
            if (_net.IsClient && _timing.IsFirstTimePredicted)
                _popup.PopupEntity(Loc.GetString("modules-no-modules"), uid, user);
            return false;
        }

        _tool.UseTool(used, user, uid, 1f, component.ModulesExtractionMethod, new ModuleRemovalFinishedEvent(),
            toolComponent: tool);
        return true;
    }


    public bool TryInsertBattery(EntityUid uid, EntityUid used, EntityUid user, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        if (component.BatterySlot.ContainedEntity != null)
            return false;

        if (component.BatterySlot.Insert(used))
        {
            _audio.PlayPredicted(component.ModuleInsertionSound, uid, user);
            var ev = new BatteryInsertedEvent(uid, used);
            RaiseLocalEvent(uid, ev);
            Dirty(component);
            return true;
        }

        _actionBlocker.UpdateCanMove(uid);
        return false;
    }


    public bool TryRemoveBattery(EntityUid uid, EntityUid used, EntityUid user, CyborgComponent component,
        ToolComponent tool)
    {
        if (!component.BatterySlot.ContainedEntity.HasValue)
        {
            if (_net.IsClient && _timing.IsFirstTimePredicted)
                _popup.PopupEntity(Loc.GetString("power-cell-no-battery"), uid, user);
            return false;
        }

        _tool.UseTool(used, user, uid, 1f, component.ModulesExtractionMethod, new RemoveBatteryEvent(),
            toolComponent: tool);
        Dirty(component);
        _actionBlocker.UpdateCanMove(uid);
        return true;
    }

    public virtual void UpdateUserInterface(EntityUid uid, CyborgComponent? component = null)
    {
    }

    public virtual bool TryChangeEnergy(EntityUid uid, FixedPoint2 delta, CyborgComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.Energy + delta < 0)
            return false;

        component.Energy = FixedPoint2.Clamp(component.Energy + delta, 0, component.MaxEnergy);
        Dirty(component);
        _actionBlocker.UpdateCanMove(uid);
        return true;
    }

    #region State Handle

    private void OnComponentHandleState(EntityUid uid, CyborgComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not CyborgComponentState state)
            return;
        component.Energy = state.Energy;
        component.MaxEnergy = state.MaxEnergy;
        component.Consumption = state.Consumption;
        component.InstrumentUids = state.InstrumentUids;
        component.Active = state.Active;
        component.PanelLocked = state.PanelLocked;
    }

    private void OnComponentGetState(EntityUid uid, CyborgComponent component, ref ComponentGetState args)
    {
        args.State = new CyborgComponentState(component.Energy, component.MaxEnergy, component.Consumption,
            component.InstrumentUids, component.Active, component.PanelLocked);
    }

    #endregion


    #region emag cyborg

    public bool TryUseEmag(EntityUid uid, EntityUid user, EntityUid target, EmagComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        if (_tag.HasTag(target, comp.EmagImmuneTag))
            return false;

        TryComp<LimitedChargesComponent>(uid, out var charges);
        if (_charges.IsEmpty(uid, charges))
        {
            if (_net.IsClient && _timing.IsFirstTimePredicted)
                _popup.PopupEntity(Loc.GetString("emag-no-charges"), user, user);
            return false;
        }

        var handled = DoEmagEffect(user, target);
        if (!handled)
            return false;

        // only do popup on client
        if (_net.IsClient && _timing.IsFirstTimePredicted)
        {
            _popup.PopupEntity(Loc.GetString("emag-success", ("target", Identity.Entity(target, EntityManager))), user,
                user, PopupType.Medium);
        }

        _adminLogger.Add(LogType.Emag, LogImpact.High,
            $"{ToPrettyString(user):player} emagged {ToPrettyString(target):target}");

        if (charges != null)
            _charges.UseCharge(uid, charges);
        return true;
    }

    public bool DoEmagEffect(EntityUid user, EntityUid target)
    {
        if (HasComp<CyborgEmaggedComponent>(target))
            return false;

        var ev = new CyborgGotEmaggedEvent(user);
        RaiseLocalEvent(target, ev);

        EnsureComp<CyborgEmaggedComponent>(target).EmmagedBy = user;
        return true;
    }

    #endregion
}

[Serializable]
[NetSerializable]
public sealed class ModuleRemovalFinishedEvent : SimpleDoAfterEvent
{
}
