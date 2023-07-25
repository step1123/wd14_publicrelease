﻿using Content.Server.Coordinates.Helpers;
using Content.Server.Maps;
using Content.Server.Popups;
using Content.Server.White.IncorporealSystem;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.StatusEffect;
using Content.Shared.White.Cult;
using Robust.Server.GameObjects;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem
{
    [Dependency] private readonly TileSystem _tileSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;




    public void InitializeConstructsAbilities()
    {
        SubscribeLocalEvent<ArtificerCreateSoulStoneActionEvent>(OnArtificerCreateSoulStone);
        SubscribeLocalEvent<ArtificerCreateConstructShellActionEvent>(OnArtificerCreateConstructShell);
        SubscribeLocalEvent<ArtificerConvertCultistFloorActionEvent>(OnArtificerConvertCultistFloor);
        SubscribeLocalEvent<ArtificerCreateCultistWallActionEvent>(OnArtificerCreateCultistWall);

        SubscribeLocalEvent<WraithPhaseActionEvent>(OnWraithPhase);
        SubscribeLocalEvent<IncorporealComponent, AttackAttemptEvent>(OnAttackAttempt);

        SubscribeLocalEvent<JuggernautCreateWallActionEvent>(OnJuggernautCreateWall);

        SubscribeLocalEvent<ConstructComponent, ComponentInit>(OnConstructInit);
    }

    private void OnConstructInit(EntityUid uid, ConstructComponent component, ComponentInit args)
    {
        foreach (var action in component.Actions)
        {
            var actionPrototype = _prototypeManager.Index<InstantActionPrototype>(action);
            _actionsSystem.AddAction(uid, new InstantAction(actionPrototype), uid);
        }
    }

    private void OnArtificerCreateSoulStone(ArtificerCreateSoulStoneActionEvent ev)
    {
        var transform = Transform(ev.Performer);
        Spawn(ev.SoulStonePrototypeId, transform.Coordinates);

        ev.Handled = true;
    }

    private void OnArtificerCreateConstructShell(ArtificerCreateConstructShellActionEvent ev)
    {
        var transform = Transform(ev.Performer);
        Spawn(ev.ShellPrototypeId, transform.Coordinates);

        ev.Handled = true;
    }

    private void OnArtificerConvertCultistFloor(ArtificerConvertCultistFloorActionEvent ev)
    {
        var transform = Transform(ev.Performer);
        var gridUid = transform.GridUid;

        if (!gridUid.HasValue)
        {
            _popupSystem.PopupEntity("Нельзя строить в космосе...", ev.Performer, ev.Performer);
            return;
        }

        var tileRef = transform.Coordinates.GetTileRef();

        if (!tileRef.HasValue)
        {
            _popupSystem.PopupEntity("Нельзя строить в космосе...", ev.Performer, ev.Performer);
            return;
        }

        var cultistTileDefinition = (ContentTileDefinition) _tileDefinition[ev.FloorTileId];
        _tileSystem.ReplaceTile(tileRef.Value, cultistTileDefinition);
        Spawn("CultTileSpawnEffect", transform.Coordinates);
        ev.Handled = true;
    }

    private void OnArtificerCreateCultistWall(ArtificerCreateCultistWallActionEvent ev)
    {
        if (!TrySpawnWall(ev.Performer, ev.WallPrototypeId))
        {
            return;
        }

        ev.Handled = true;
    }

    private void OnWraithPhase(WraithPhaseActionEvent ev)
    {
        if (_statusEffectsSystem.HasStatusEffect(ev.Performer, ev.StatusEffectId))
        {
            _popupSystem.PopupEntity("Вы уже в потустороннем мире", ev.Performer, ev.Performer);
            return;
        }

        _statusEffectsSystem.TryAddStatusEffect<IncorporealComponent>(ev.Performer, ev.StatusEffectId, TimeSpan.FromSeconds(ev.Duration), false);

        ev.Handled = true;
    }

    private void OnAttackAttempt(EntityUid uid, IncorporealComponent component, AttackAttemptEvent args)
    {
        if (_statusEffectsSystem.HasStatusEffect(args.Uid, "Incorporeal"))
        {
            _statusEffectsSystem.TryRemoveStatusEffect(args.Uid, "Incorporeal");
        }
    }

    private void OnJuggernautCreateWall(JuggernautCreateWallActionEvent ev)
    {
        if (!TrySpawnWall(ev.Performer, ev.WallPrototypeId))
        {
            return;
        }

        ev.Handled = true;
    }

    private bool TrySpawnWall(EntityUid performer, string wallPrototypeId)
    {
        var xform = Transform(performer);

        var offsetValue = xform.LocalRotation.ToWorldVec().Normalized;
        var coords = xform.Coordinates.Offset(offsetValue).SnapToGrid(EntityManager);
        var tile = coords.GetTileRef();
        if (tile == null)
            return false;

        // Check there are no walls there
        if (_turf.IsTileBlocked(tile.Value, CollisionGroup.Impassable))
        {
            _popupSystem.PopupEntity(Loc.GetString("mime-invisible-wall-failed"), performer, performer);
            return false;
        }

        // Check there are no mobs there
        foreach (var entity in _lookupSystem.GetEntitiesIntersecting(tile.Value))
        {
            if (HasComp<MobStateComponent>(entity) && entity != performer)
            {
                _popupSystem.PopupEntity(Loc.GetString("mime-invisible-wall-failed"), performer, performer);
                return false;
            }
        }
        _popupSystem.PopupEntity(Loc.GetString("mime-invisible-wall-popup", ("mime", performer)), performer);
        // Make sure we set the invisible wall to despawn properly
        Spawn(wallPrototypeId, coords);
        return true;
    }
}
