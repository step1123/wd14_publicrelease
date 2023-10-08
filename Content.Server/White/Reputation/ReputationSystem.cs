using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Server.Mind.Components;
using Content.Server.UtkaIntegration;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Server.White.Reputation;

public sealed class ReputationSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ReputationManager _repManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly IPlayerLocator _locator = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UtkaBannedEvent>(ModifyReputationOnPlayerBanned);
    }

    /// <summary>
    /// Tries to modify reputation on round end and then returns it's new value and delta value if successful.
    /// </summary>
    /// <param name="name">Player to get new values for.</param>
    /// <param name="newValue">Modified player's reputation value.</param>
    /// <param name="deltaValue"></param>
    /// <returns>Success in modifying player's reputation.</returns>
    public bool TryModifyReputationOnRoundEnd(string name, out float? newValue, out float? deltaValue)
    {
        newValue = null;
        deltaValue = null;

        if (!_playerManager.TryGetSessionByUsername(name, out var session) || session.AttachedEntity == null)
            return false;

        if (!TryCalculatePlayerReputation(session.AttachedEntity.Value, out var delta))
            return false;

        var uid = session.UserId;
        _repManager.GetCachedPlayerReputation(uid, out var value);

        if (value == null)
            return false;

        var longRound = _gameTicker.RoundDuration().Minutes >= 25;
        if (delta != 0 && longRound)
        {
            _repManager.ModifyPlayerReputation(uid, delta);
        }

        deltaValue = longRound ? delta : 0f;
        newValue = value + deltaValue;

        return true;
    }

    private bool TryCalculatePlayerReputation(EntityUid entity, out float deltaValue)
    {
        deltaValue = 0f;
        var aspect = false;

        if (!TryComp<MobStateComponent>(entity, out var state) || state.CurrentState is MobState.Dead or MobState.Invalid)
            return true;

        var ruleEnt = _gameTicker.GetActiveGameRules()
            .Where(HasComp<AspectComponent>)
            .FirstOrNull();

        if (ruleEnt != null)
        {
            if (TryComp<AspectComponent>(ruleEnt, out var comp))
            {
                deltaValue += comp.Weight switch
                {
                    3 => 2f,
                    2 => 3f,
                    1 => 4f,
                    _ => 0f
                };
                aspect = true;
            }
        }

        if (!aspect)
            deltaValue += 1f;

        if (TryComp<MindContainerComponent>(entity, out var mind)
            && mind.Mind != null
            && mind.Mind.AllRoles.Any(role => role.Antagonist))
        {
            var condCompleted = 0;
            var totalCond = 0;
            foreach (var obj in mind.Mind.AllObjectives)
            {
                foreach (var condition in obj.Conditions)
                {
                    totalCond++;

                    if (condition.Progress > 0.99f)
                        condCompleted++;
                }
            }

            if (aspect)
            {
                if (condCompleted == totalCond)
                    deltaValue += 1f + condCompleted;
                else
                    deltaValue += 1f + condCompleted * 0.5f;
            }
            else
            {
                if (condCompleted == totalCond)
                    deltaValue += 2f + condCompleted * 0.5f;
                else
                    deltaValue += condCompleted * 0.5f;
            }
        }

        return true;
    }

    private async void ModifyReputationOnPlayerBanned(UtkaBannedEvent ev)
    {
        NetUserId uid;
        float value;

        if (ev.Bantype == "server")
        {
            value = ev.Duration switch
            {
                > 10080 => -10f,
                > 4320 => -7f,
                > 1440 => -5f,
                0 => -25f,
                _ => -3f
            };
        }
        else
            value = -2f;

        if (_playerManager.TryGetPlayerDataByUsername(ev.Ckey!, out var data))
            uid = data.UserId;
        else
        {
            var located = await _locator.LookupIdByNameAsync(ev.Ckey!);

            if (located == null)
                return;

            uid = located.UserId;
        }

        _repManager.ModifyPlayerReputation(uid, value);
    }
}
