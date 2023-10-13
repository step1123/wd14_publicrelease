﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Content.Client.Administration.Managers;
using Content.Shared.CCVar;
using Content.Shared.Players;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Content.Shared.White.JobWhitelist;
using Robust.Client;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client.Players.PlayTimeTracking;

public sealed class JobRequirementsManager
{
    [Dependency] private readonly IBaseClient _client = default!;
    [Dependency] private readonly IClientNetManager _net = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IClientAdminManager _adminManager = default!; // WD

    private readonly Dictionary<string, TimeSpan> _roles = new();
    private readonly List<string> _roleBans = new();
    private readonly List<string> _allowedJobs = new(); // WD EDIT

    private ISawmill _sawmill = default!;

    public event Action? Updated;

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("job_requirements");

        // Yeah the client manager handles role bans and playtime but the server ones are separate DEAL.
        _net.RegisterNetMessage<MsgRoleBans>(RxRoleBans);
        _net.RegisterNetMessage<MsgPlayTime>(RxPlayTime);
        _net.RegisterNetMessage<MsgJobWL>(RxJobWL); // WD EDIT

        _client.RunLevelChanged += ClientOnRunLevelChanged;
        _adminManager.AdminStatusUpdated += () => Updated?.Invoke(); // WD
    }

    private void ClientOnRunLevelChanged(object? sender, RunLevelChangedEventArgs e)
    {
        if (e.NewLevel == ClientRunLevel.Initialize)
        {
            // Reset on disconnect, just in case.
            _roles.Clear();
        }
    }

    // WD EDIT start
    private void RxJobWL(MsgJobWL message)
    {
        _sawmill.Debug($"Received jobwhitelist info containing {message.AllowedJobs.Count} entries.");

        if (_allowedJobs.Equals(message.AllowedJobs))
            return;

        _allowedJobs.Clear();
        _allowedJobs.AddRange(message.AllowedJobs);
        Updated?.Invoke();
    }
    // WD EDIT end

    private void RxRoleBans(MsgRoleBans message)
    {
        _sawmill.Debug($"Received roleban info containing {message.Bans.Count} entries.");

        if (_roleBans.Equals(message.Bans))
            return;

        _roleBans.Clear();
        _roleBans.AddRange(message.Bans);
        Updated?.Invoke();
    }

    private void RxPlayTime(MsgPlayTime message)
    {
        _roles.Clear();

        // NOTE: do not assign _roles = message.Trackers due to implicit data sharing in integration tests.
        foreach (var (tracker, time) in message.Trackers)
        {
            _roles[tracker] = time;
        }

        /*var sawmill = Logger.GetSawmill("play_time");
        foreach (var (tracker, time) in _roles)
        {
            sawmill.Info($"{tracker}: {time}");
        }*/
        Updated?.Invoke();
    }

    public bool IsAllowed(JobPrototype job, [NotNullWhen(false)] out string? reason)
    {
        reason = null;

        if (_roleBans.Contains($"Job:{job.ID}"))
        {
            reason = Loc.GetString("role-ban");
            return false;
        }

        if (job.Requirements == null)
            return true;

        // WD EDIT start
        if (!job.Requirements.Any(req => req is WLRequirement))
        {
            if (!_cfg.GetCVar(CCVars.GameRoleTimers) || _adminManager.IsActive())
                return true;
        }
        // WD EDIT end

        var player = _playerManager.LocalPlayer?.Session;

        if (player == null)
            return true;

        if (_allowedJobs.Contains(job.ID)) // WD EDIT
            return true;



        var reasonBuilder = new StringBuilder();

        var first = true;
        foreach (var requirement in job.Requirements)
        {
            // WD EDIT start
            if (JobRequirements.TryRequirementMet(requirement, _roles, _allowedJobs, out reason, _prototypes))
                continue;
            if (requirement is WLRequirement && job.Requirements.Count > 1)
            {
                reasonBuilder.AppendLine(Loc.GetString("jobwhitelist-or-required")+"\n");
                continue;
            }
            // WD EDIT end

            if (!first)
                reasonBuilder.Append('\n');
            first = false;

            reasonBuilder.AppendLine(reason);
        }

        reason = reasonBuilder.Length == 0 ? null : reasonBuilder.ToString();
        return reason == null;
    }
}
