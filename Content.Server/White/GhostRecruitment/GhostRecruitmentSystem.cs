using System.Linq;
using Content.Server.EUI;
using Content.Server.Ghost.Components;
using Content.Server.Humanoid.Components;
using Content.Server.Mind;
using Content.Server.Players;
using Content.Shared.White.GhostRecruitment;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.White.GhostRecruitment;

public sealed class GhostRecruitmentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public void StartRecruitment(string recruitmentName)
    {
        var query = EntityQueryEnumerator<GhostComponent,ActorComponent>();
        while (query.MoveNext(out var uid,out _,out var actorComponent))
        {
            _eui.OpenEui(new GhostRecruitmentEuiAccept(uid,recruitmentName,this),actorComponent.PlayerSession);
        }
    }

    public void Recruit(EntityUid uid,string recruitmentName)
    {
        EnsureComp<GhostRecruitedComponent>(uid).RecruitmentName = recruitmentName;
    }

    public bool EndRecruitment(string recruitmentName)
    {
        var query = EntityQueryEnumerator<GhostRecruitedComponent>();

        var spawners = GetEventSpawners(recruitmentName).ToList();
        _random.Shuffle(spawners);
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
            if(!HasComp<ActorComponent>(uid) || count >= spawners.Count)
                continue;

            var (spawnerUid, spawnerComponent) = spawners[count];

            TransferMind(uid,spawnerUid,spawnerComponent);
            count++;

            EnsureComp<RecruitedComponent>(uid).RecruitmentName = recruitmentName;

            var ghostEvent = new GhostRecruitmentSuccessEvent(recruitmentName);
            RaiseLocalEvent(uid,ghostEvent);
        }

        var ghostsEvent = new GhostsRecruitmentSuccessEvent(recruitmentName);
        RaiseLocalEvent(ghostsEvent);
        return true;
    }

    public void TransferMind(EntityUid from,EntityUid spawnerUid,GhostRecruitmentSpawnPointComponent? component = null)
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

    public EntityUid? Spawn(EntityUid spawnerUid,GhostRecruitmentSpawnPointComponent? component = null)
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
}

