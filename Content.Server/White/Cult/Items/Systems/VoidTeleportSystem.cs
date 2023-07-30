﻿using System.Threading;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Pulling.Components;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Items;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.White.Cult.Items.Systems;

public sealed class VoidTeleportSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidTeleportComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<VoidTeleportComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, VoidTeleportComponent component, ComponentInit args)
    {
        UpdateAppearance(uid, component);
    }

    private void OnUseInHand(EntityUid uid, VoidTeleportComponent component, UseInHandEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
        {
            _hands.TryDrop(args.User);
            _popup.PopupEntity(Loc.GetString("void-teleport-not-cultist"), args.User, args.User);
            return;
        }

        if (!component.Active || component.UsesLeft <= 0)
        {
            _popup.PopupEntity(Loc.GetString("void-teleport-drained"), args.User, args.User);
            return;
        }

        if (component.NextUse > _timing.CurTime)
        {
            _popup.PopupEntity(Loc.GetString("void-teleport-cooldown"), args.User, args.User);
            return;
        }

        if (!TryComp<TransformComponent>(args.User, out var transform))
            return;

        var oldCoords = transform.Coordinates;

        EntityCoordinates coords = default;
        var attempts = 10;
        //Repeat until proper place for tp is found
        while (attempts <= 10)
        {
            attempts--;
            //Get coords to where tp
            var random = new Random().Next(component.MinRange, component.MaxRange);
            var offset = transform.LocalRotation.ToWorldVec().Normalized();
            var direction = transform.LocalRotation.GetDir().ToVec();
            var newOffset = offset + direction * random;
            coords = transform.Coordinates.Offset(newOffset).SnapToGrid(EntityManager);

            var tile = coords.GetTileRef();

            //Check for walls
            if (tile != null && _turf.IsTileBlocked(tile.Value, CollisionGroup.AllMask))
                continue;

            break;
        }

        CreatePulse(uid, component);

        _xform.SetCoordinates(args.User, coords);
        transform.AttachToGridOrMap();

        var pulled = GetPulledEntity(args.User);
        if (pulled != null)
        {
            _xform.SetCoordinates(pulled.Value, coords);

            if (TryComp<TransformComponent>(pulled.Value, out var pulledTransform))
                pulledTransform.AttachToGridOrMap();
        }

        //Play tp sound
        _audio.PlayPvs(component.TeleportInSound, coords);
        _audio.PlayPvs(component.TeleportOutSound,oldCoords);

        //Create tp effect
        _entMan.SpawnEntity(component.TeleportInEffect, coords);
        _entMan.SpawnEntity(component.TeleportOutEffect, oldCoords);

        component.UsesLeft--;
        component.NextUse = _timing.CurTime + component.Cooldown;
    }

    private void UpdateAppearance(EntityUid uid, VoidTeleportComponent comp)
    {
        AppearanceComponent? appearance = null;
        if (!Resolve(uid, ref appearance, false))
            return;

        _appearance.SetData(uid, VeilVisuals.Activated, comp.Active, appearance);
    }

    private EntityUid? GetPulledEntity(EntityUid user)
    {
        EntityUid? pulled = null;

        if (TryComp<SharedPullerComponent>(user, out var puller))
            pulled = puller.Pulling;

        return pulled;
    }

    private void CreatePulse(EntityUid uid, VoidTeleportComponent component)
    {
        if (TryComp<PointLightComponent>(uid, out var light))
            light.Energy = 5f;

        Timer.Spawn(component.TimerDelay, () => TurnOffPulse(uid, component), component.Token.Token);
    }

    private void TurnOffPulse(EntityUid uid ,VoidTeleportComponent comp)
    {
        if (!TryComp<PointLightComponent>(uid, out var light))
            return;

        light.Energy = 1f;

        comp.Token = new CancellationTokenSource();

        if (comp.UsesLeft <= 0)
        {
            comp.Active = false;
            UpdateAppearance(uid, comp);

            light.Enabled = false;
        }
    }
}
