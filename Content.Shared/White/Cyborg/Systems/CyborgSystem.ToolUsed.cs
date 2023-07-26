using Content.Shared.Access.Components;
using Content.Shared.Emag.Components;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.Tools.Components;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.White.Cyborg.Systems;

/// <summary>
///     responsible for the action of the tool to the borg
/// </summary>
public sealed class CyborgSystemToolUsed : EntitySystem
{
    [Dependency] private readonly SharedCyborgSystem _cyborg = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EmagComponent, ToolUseOnBorgEvent>(OnEmmaged);
        SubscribeLocalEvent<CyborgModuleComponent, ToolUseOnBorgEvent>(OnModule);
        SubscribeLocalEvent<AccessComponent, ToolUseOnBorgEvent>(OnAccess);
        SubscribeLocalEvent<ToolComponent, ToolUseOnBorgEvent>(OnTool);
        SubscribeLocalEvent<PowerCellComponent, ToolUseOnBorgEvent>(OnBattery);
    }

    private void OnBattery(EntityUid uid, PowerCellComponent component, ToolUseOnBorgEvent args)
    {
        if (args.Handled || !TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent))
            return;

        args.Handled = _cyborg.TryInsertBattery(args.CyborgUid, uid, args.User, cyborgComponent);
    }

    private void OnTool(EntityUid uid, ToolComponent component, ToolUseOnBorgEvent args)
    {
        if (args.Handled || !TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent))
            return;

        if (!component.Qualities.Contains(cyborgComponent.ModulesExtractionMethod))
            return;

        if (cyborgComponent.PanelLocked)
        {
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("cyborg-panel-locked"), args.CyborgUid, args.User);
            return;
        }

        if (cyborgComponent.BatterySlot.ContainedEntity.HasValue)
            args.Handled = _cyborg.TryRemoveBattery(args.CyborgUid, args.Used, args.User, cyborgComponent, component);
        else if (cyborgComponent.ModuleContainer.ContainedEntities.Count > 0)
            args.Handled = _cyborg.TryRemoveModule(args.CyborgUid, args.Used, args.User, cyborgComponent, component);
    }

    private void OnAccess(EntityUid uid, AccessComponent component, ToolUseOnBorgEvent args)
    {
        if (args.Handled || !TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent) ||
            TryComp<CyborgModuleComponent>(uid, out _))
            return;

        if (_cyborg.HasAccess(args.CyborgUid, component.Tags, cyborgComponent))
        {
            var isLocked = _cyborg.TogglePanelLock(args.CyborgUid, cyborgComponent);
            if (!_timing.IsFirstTimePredicted)
                return;

            if (isLocked && _net.IsServer)
                _popup.PopupEntity(Loc.GetString("cyborg-lock-success"), args.CyborgUid, args.User);

            else if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("cyborg-unlock-success"), args.CyborgUid, args.User);

            var ev = new PanelLockedEvent(isLocked);
            RaiseLocalEvent(args.CyborgUid, ev);
        }
        else if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("cyborg-panel-unsuccess"), args.CyborgUid, args.User);

        args.Handled = true;
    }

    private void OnModule(EntityUid uid, CyborgModuleComponent component, ToolUseOnBorgEvent args)
    {
        if (args.Handled || !TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent))
            return;

        if (cyborgComponent.PanelLocked)
        {
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("cyborg-panel-locked"), args.CyborgUid, args.User);
            return;
        }

        args.Handled = _cyborg.TryInsertModule(args.CyborgUid, args.Used, args.User, cyborgComponent, component);
    }

    private void OnEmmaged(EntityUid uid, EmagComponent component, ToolUseOnBorgEvent args)
    {
        if (args.Handled || !TryComp<CyborgComponent>(args.CyborgUid, out var cyborgComponent))
            return;

        if (_cyborg.TryUseEmag(uid, args.User, args.CyborgUid, component))
        {
            cyborgComponent.PanelLocked = false;
            //_audio.PlayPvs(component.SparkSound, uid, AudioParams.Default.WithVolume(8));
            args.Handled = true;
        }
    }
}
