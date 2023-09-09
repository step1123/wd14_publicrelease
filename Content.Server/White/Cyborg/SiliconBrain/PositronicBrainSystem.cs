using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind.Components;
using Content.Server.Popups;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.White.Cyborg.SiliconBrain;
using Content.Shared.White.Cyborg.SiliconBrain.Components;
using Content.Shared.White.Cyborg.SiliconBrain.Systems;

namespace Content.Server.White.Cyborg.SiliconBrain;

public sealed class PositronicBrainSystem : SharedPositronicBrainSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PositronicBrainComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PositronicBrainComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<PositronicBrainComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<PositronicBrainComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<PositronicBrainComponent, GetVerbsEvent<ActivationVerb>>(AddWipeVerb);
        SubscribeLocalEvent<PositronicBrainComponent, BrainInsertEvent>(OnInserted);
        SubscribeLocalEvent<SiliconBrainContainerComponent, MindAddedMessage>(OnMindAddedParent);
    }

    private void OnMindAddedParent(EntityUid uid, SiliconBrainContainerComponent component, MindAddedMessage args)
    {
        if (!component.BrainUid.HasValue || !HasComp<PositronicBrainComponent>(component.BrainUid.Value))
            return;

        PositronicBrainTurningOff(component.BrainUid.Value);
    }

    private void OnInserted(EntityUid uid, PositronicBrainComponent component, BrainInsertEvent args)
    {
        if (!TryComp<SiliconBrainComponent>(uid, out var siliconBrainComponent) ||
            !siliconBrainComponent.ParentUid.HasValue ||
            HasComp<MindContainerComponent>(siliconBrainComponent.ParentUid.Value))
            return;

        SearchMind(uid);
    }

    private void OnExamined(EntityUid uid, PositronicBrainComponent component, ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
        {
            if (TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
                args.PushMarkup(Loc.GetString("positronic-brain-occupied"));
            else if (HasComp<GhostTakeoverAvailableComponent>(uid))
                args.PushMarkup(Loc.GetString("positronic-brain-still-searching"));
            else
                args.PushMarkup(Loc.GetString("positronic-brain-standny"));
        }
    }

    private void OnUseInHand(EntityUid uid, PositronicBrainComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        SearchMind(uid, args.User, component);
    }

    private void OnMindRemoved(EntityUid uid, PositronicBrainComponent component, MindRemovedMessage args)
    {
        PositronicBrainTurningOff(uid);
    }

    private void OnMindAdded(EntityUid uid, PositronicBrainComponent component, MindAddedMessage args)
    {
        RemComp<GhostTakeoverAvailableComponent>(uid);
        UpdatePositronicBrainAppearance(uid, PosiStatus.Occupied);
    }

    private void PositronicBrainTurningOff(EntityUid uid, PositronicBrainComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        RemComp<GhostTakeoverAvailableComponent>(uid);
        RemComp<GhostRoleComponent>(uid);
        RemComp<MindContainerComponent>(uid);
        UpdatePositronicBrainAppearance(uid, PosiStatus.Standby);
    }

    private void UpdatePositronicBrainAppearance(EntityUid uid, PosiStatus status)
    {
        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearance.SetData(uid, PosiVisuals.Status, status, appearance);
    }

    private void AddWipeVerb(EntityUid uid, PositronicBrainComponent component, GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
        {
            ActivationVerb verb = new();
            verb.Text = Loc.GetString("positronic-brain-wipe-verb-text");
            verb.Act = () =>
            {
                _popupSystem.PopupEntity(Loc.GetString("positronic-brain-wiped"), uid, args.User, PopupType.Large);
                PositronicBrainTurningOff(uid);
            };
            args.Verbs.Add(verb);
        }
        else if (HasComp<GhostTakeoverAvailableComponent>(uid))
        {
            ActivationVerb verb = new();
            verb.Text = Loc.GetString("positronic-brain-stop-searching-verb-text");
            verb.Act = () =>
            {
                _popupSystem.PopupEntity(Loc.GetString("positronic-brain-stopped-searching"), uid, args.User);
                PositronicBrainTurningOff(uid);
            };
            args.Verbs.Add(verb);
        }
    }

    public void SearchMind(EntityUid uid, EntityUid? user = null, PositronicBrainComponent? component = null,
        MindContainerComponent? mind = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (Resolve(uid, ref mind, false) && mind.HasMind && user.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("positronic-brain-occupied"), uid, user.Value, PopupType.Large);
            return;
        }

        if (HasComp<GhostTakeoverAvailableComponent>(uid) && user.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("positronic-brain-still-searching"), uid, user.Value);
            return;
        }


        var ghostRole = EnsureComp<GhostRoleComponent>(uid);
        EnsureComp<GhostTakeoverAvailableComponent>(uid);

        ghostRole.RoleName = Loc.GetString("positronic-brain-role-name");
        ghostRole.RoleDescription = Loc.GetString("positronic-brain-role-description");
        ghostRole.RoleRules = Loc.GetString("positronic-brain-role-description");

        if (user.HasValue)
            _popupSystem.PopupEntity(Loc.GetString("positronic-brain-searching"), uid, user.Value);

        UpdatePositronicBrainAppearance(uid, PosiStatus.Searching);
    }
}
