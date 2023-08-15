using System.Linq;
using Content.Server.EUI;
using Content.Server.Ghost.Components;
using Content.Server.Humanoid.Components;
using Content.Server.Mind;
using Content.Server.Players;
using Content.Shared.White.GhostRecruitment;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Random;

namespace Content.Server.White.GhostRecruitment;

/// <summary>
/// responsible for recruiting guests for all sorts of roles
/// </summary>
public sealed class GhostRecruitmentSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    private readonly Dictionary<IPlayerSession, GhostRecruitmentEuiAccept> _openUis = new();

    /// <summary>
    /// starts recruiting ghosts, showing them a menu with a choice to recruit.
    /// </summary>
    /// <param name="recruitmentName">name of recruitment. <see cref="GhostRecruitmentSpawnPointComponent"/></param>
    public void StartRecruitment(string recruitmentName)
    {
        var query = EntityQueryEnumerator<GhostComponent,ActorComponent>();
        while (query.MoveNext(out var uid,out _,out var actorComponent))
        {
            OpenEui(uid,recruitmentName,actorComponent);
        }
    }

    /// <summary>
    /// if the ghost agrees, then he queues up for the role
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="recruitmentName">name of recruitment. <see cref="GhostRecruitmentSpawnPointComponent"/></param>
    public void Recruit(EntityUid uid,string recruitmentName)
    {
        EnsureComp<GhostRecruitedComponent>(uid).RecruitmentName = recruitmentName;
    }

    /// <summary>
    /// Arranges the ghosts that agreed by roles.
    /// </summary>
    /// <param name="recruitmentName">name of recruitment. <see cref="GhostRecruitmentSpawnPointComponent"/></param>
    /// <returns>is success?</returns>
    public bool EndRecruitment(string recruitmentName)
    {
        var query = EntityQueryEnumerator<GhostRecruitedComponent>();

        var spawners = GetEventSpawners(recruitmentName).ToList();

        // We prioritize the queue, for example, the commander first, and then the engineer
        spawners = spawners.OrderBy(o => o.Item2.Priority).ToList();

        var count = 0;

        var attemptRecruitment = new GhostRecruitmentAttemptEvent(recruitmentName);
        RaiseLocalEvent(attemptRecruitment);

        if (attemptRecruitment.Cancelled)
        {
            var failEvent = new GhostsRecruitmentFailEvent(recruitmentName);
            RaiseLocalEvent(failEvent);

            return false;
        }

        while (query.MoveNext(out var uid,out var ghostRecruitedComponent))
        {
            if(ghostRecruitedComponent.RecruitmentName != recruitmentName)
                continue;

            RemComp<GhostRecruitedComponent>(uid);

            if (!TryComp<ActorComponent>(uid, out var actorComponent))
                continue;

            CloseEui(uid,recruitmentName,actorComponent);

            // if there are too many recruited, then just skip
            if(count >= spawners.Count)
                continue;

            var (spawnerUid, spawnerComponent) = spawners[count];

            TransferMind(uid,spawnerUid,spawnerComponent);
            count++;

            EnsureComp<RecruitedComponent>(uid).RecruitmentName = recruitmentName;

            var ghostEvent = new GhostRecruitmentSuccessEvent(recruitmentName,actorComponent.PlayerSession);
            RaiseLocalEvent(uid,ghostEvent);
        }

        var ghostsEvent = new GhostsRecruitmentSuccessEvent(recruitmentName);
        RaiseLocalEvent(ghostsEvent);
        return true;
    }

    private void TransferMind(EntityUid from,EntityUid spawnerUid,GhostRecruitmentSpawnPointComponent? component = null)
    {
        if (!Resolve(spawnerUid, ref component) || !TryComp<ActorComponent>(from,out var actorComponent))
            return;

        var entityUid = Spawn(spawnerUid, component);

        if(!entityUid.HasValue)
            return;
        var mind = actorComponent.PlayerSession.GetMind()!;

        _mind.TransferTo(mind,entityUid.Value);
        _mind.UnVisit(mind);
    }

    private EntityUid? Spawn(EntityUid spawnerUid,GhostRecruitmentSpawnPointComponent? component = null)
    {
        if (!Resolve(spawnerUid, ref component))
            return null;

        var uid = EntityManager.SpawnEntity(component.EntityPrototype, Transform(spawnerUid).Coordinates);

        if (HasComp<RandomHumanoidSpawnerComponent>(uid))
        {
            uid = new EntityUid((int) uid + 1);
        }

        return uid;
    }

    public IEnumerable<(EntityUid, GhostRecruitmentSpawnPointComponent)> GetEventSpawners(string recruitmentName)
    {
        var query = EntityQueryEnumerator<GhostRecruitmentSpawnPointComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.RecruitmentName == recruitmentName)
                yield return (uid, component);
        }
    }

    public IEnumerable<(EntityUid, GhostRecruitedComponent)> GetAllRecruited(string recruitmentName)
    {
        var query = EntityQueryEnumerator<GhostRecruitedComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.RecruitmentName == recruitmentName)
                yield return (uid, component);
        }
    }

    public void OpenEui(EntityUid uid,string recruitmentName,ActorComponent? actorComponent = null)
    {
        if(!Resolve(uid,ref actorComponent))
            return;
        var eui = new GhostRecruitmentEuiAccept(uid, recruitmentName, this);

        _openUis.Add(actorComponent.PlayerSession,eui);
        _eui.OpenEui(eui,actorComponent.PlayerSession);
    }

    public void CloseEui(EntityUid uid,string recruitmentName,ActorComponent? actorComponent = null)
    {
        if(!Resolve(uid,ref actorComponent))
            return;

        var session = actorComponent.PlayerSession;

        if (!_openUis.ContainsKey(session))
            return;

        _openUis.Remove(session, out var eui);

        eui?.Close();
    }
}

