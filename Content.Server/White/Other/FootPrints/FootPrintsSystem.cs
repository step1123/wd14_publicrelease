using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Decals;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.White.Other.FootPrints;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.White.Other.FootPrints;

public sealed class FootPrintsSystem : EntitySystem
{
    [Dependency] private readonly DecalSystem _decals = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    private List<KeyValuePair<EntityUid, uint>> _storedDecals = new();
    private const int MaxStoredDecals = 1000 ;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FootPrintsComponent, ComponentStartup>(OnStartupComponent);
        SubscribeLocalEvent<FootPrintsComponent, MoveEvent>(OnMove);
    }

    private void OnStartupComponent(EntityUid uid, FootPrintsComponent comp, ComponentStartup args)
    {
        comp.StepSize += _random.NextFloat(-0.05f, 0.05f);
    }

    private void OnMove(EntityUid uid, FootPrintsComponent comp, ref MoveEvent args)
    {
        if (comp.PrintsColor.A <= 0f)
            return;

        if (!TryComp<TransformComponent>(comp.Owner, out var transform))
            return;

        if (!TryComp<MobThresholdsComponent>(comp.Owner, out var mobThreshHolds))
            return;

        var dragging = mobThreshHolds.CurrentThresholdState is MobState.Critical or MobState.Dead;
        var distance = (transform.LocalPosition - comp.StepPos).Length();
        var stepSize = dragging ? comp.DragSize : comp.StepSize;

        if (distance > stepSize)
        {
            comp.RightStep = !comp.RightStep;

            if (_storedDecals.Count > MaxStoredDecals)
            {
                for (var i = 0; i < 1; i++)
                {
                    var decalToRemove = _storedDecals[i];
                    if (!_decals.RemoveDecal(decalToRemove.Key, decalToRemove.Value))
                    {
                        var randomDecalIndex = _random.Next(_storedDecals.Count);
                        var randomDecal = _storedDecals[randomDecalIndex];
                        _decals.RemoveDecal(randomDecal.Key, randomDecal.Value);
                    }
                }
                _storedDecals = _storedDecals.Skip(1).ToList();
            }

            _decals.TryAddDecal(
                PickPrint(comp, dragging),
                CalcCoords(comp, transform, dragging),
                out var decalId, comp.PrintsColor,
                dragging ? (transform.LocalPosition - comp.StepPos).ToAngle() + Angle.FromDegrees(-90f) : transform.LocalRotation + Angle.FromDegrees(180f),
                0, true);

            if (transform.GridUid != null)
                _storedDecals.Add(new KeyValuePair<EntityUid, uint>(transform.GridUid.Value, decalId));

            comp.PrintsColor = comp.PrintsColor.WithAlpha(ReduceAlpha(comp.PrintsColor.A, comp.ColorReduceAlpha));
            comp.StepPos = transform.LocalPosition;
        }
    }

    private EntityCoordinates CalcCoords(FootPrintsComponent comp, TransformComponent transform, bool state)
    {
        if (state)
            return new EntityCoordinates(comp.Owner, transform.LocalPosition + comp.OffsetCenter);

        var offset = comp.OffsetCenter + (comp.RightStep
            ? new Angle(Angle.FromDegrees(180f) + transform.LocalRotation).RotateVec(comp.OffsetPrint)
            : new Angle(transform.LocalRotation).RotateVec(comp.OffsetPrint));

        return new EntityCoordinates(comp.Owner, transform.LocalPosition + offset);
    }

    private string PickPrint(FootPrintsComponent comp, bool state)
    {
        var res = comp.RightStep ? comp.RightBarePrint : comp.LeftBarePrint;

        if (_inventorySystem.TryGetSlotEntity(comp.Owner, "shoes", out _))
        {
            res = comp.ShoesPrint;
        }

        if (_inventorySystem.TryGetSlotEntity(comp.Owner, "outerClothing", out var suit) && TryComp<PressureProtectionComponent>(suit, out _))
        {
            res = comp.SuitPrint;
        }

        if (state)
            res = _random.Pick(comp.DraggingPrint);

        return res;
    }

    private float ReduceAlpha(float alpha, float reductionAmount)
    {
        if (alpha - reductionAmount > 0f)
        {
            alpha -= reductionAmount;
        }
        else
        {
            alpha = 0f;
        }

        return alpha;
    }
}
