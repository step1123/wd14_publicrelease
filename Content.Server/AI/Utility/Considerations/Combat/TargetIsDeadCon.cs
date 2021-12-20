﻿using Content.Server.AI.WorldState;
using Content.Server.AI.WorldState.States;
using Content.Shared.MobState.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.AI.Utility.Considerations.Combat
{
    public sealed class TargetIsDeadCon : Consideration
    {
        protected override float GetScore(Blackboard context)
        {
            var target = context.GetState<TargetEntityState>().GetValue();

            if (!IoCManager.Resolve<IEntityManager>().TryGetComponent(target, out MobStateComponent? mobState))
            {
                return 0.0f;
            }

            if (mobState.IsDead())
            {
                return 1.0f;
            }

            return 0.0f;
        }
    }
}
