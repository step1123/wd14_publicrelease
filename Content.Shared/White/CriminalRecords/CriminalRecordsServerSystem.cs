using Content.Shared.StationRecords;
using Robust.Shared.GameStates;

namespace Content.Shared.White.CriminalRecords;

public sealed class CriminalRecordsServerSystem: EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EventGetCache>(OnGetCache);
        SubscribeLocalEvent<EventChangeCache>(OnChangeCache);
        SubscribeLocalEvent<EventChangeReason>(OnChangeReason);
        SubscribeLocalEvent<EventCheckServer>(OnCheckServer);
        SubscribeLocalEvent<CriminalRecordsServerComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<CriminalRecordsServerComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnGetState(EntityUid uid, CriminalRecordsServerComponent component, ref ComponentGetState args)
    {
        args.State = new CriminalRecordsServerComponent.CriminalRecordsServerComponentState(component.Cache);
    }

    private void OnHandleState(EntityUid uid, CriminalRecordsServerComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not CriminalRecordsServerComponent.CriminalRecordsServerComponentState state)
            return;

        component.Cache = state.Cache;
    }

    private void OnGetCache(EventGetCache ev)
    {
        var serverList = EntityQuery<CriminalRecordsServerComponent>();
        foreach (var server in serverList)
        {
            ev.Cache = server.Cache;
            break;
        }
    }

    private void OnChangeCache(EventChangeCache ev)
    {
        var serverList = EntityQuery<CriminalRecordsServerComponent>();
        foreach (var server in serverList)
        {
            foreach (var (key, info) in server.Cache)
            {
                if (key.ID == ev.Key.ID)
                {
                    info.Reason = ev.Record.Reason;
                    info.CriminalType = ev.Record.CriminalType;
                    Dirty(server);
                    return;
                }
            }
            server.Cache.Add(ev.Key, ev.Record);
            Dirty(server);
            return;
        }
    }

    private void OnChangeReason(EventChangeReason ev)
    {
        var serverList = EntityQuery<CriminalRecordsServerComponent>();
        foreach (var server in serverList)
        {
            foreach (var (key, info) in server.Cache)
            {
                if (key.ID == ev.Key.ID)
                {
                    info.Reason = ev.Text;
                    Dirty(server);
                }
            }
            return;
        }
    }

    private void OnCheckServer(EventCheckServer ev)
    {
        var serverList = EntityQuery<CriminalRecordsServerComponent>();
        foreach (var server in serverList)
        {
            ev.Result = true;
            return;
        }
        ev.Result = false;
    }
}

// Events
public sealed class EventGetCache
{
    public Dictionary<StationRecordKey, CriminalRecordInfo> Cache = new();

    public EventGetCache()
    {
    }
}

public sealed class EventChangeCache
{
    public CriminalRecordInfo Record { get; }
    public StationRecordKey Key { get; }

    public EventChangeCache(StationRecordKey key, CriminalRecordInfo record)
    {
        Key = key;
        Record = record;
    }
}

public sealed class EventChangeReason
{
    public string Text { get; }
    public StationRecordKey Key { get; }

    public EventChangeReason(StationRecordKey key, string text)
    {
        Key = key;
        Text = text;
    }
}

public sealed class EventCheckServer
{
    public bool Result { get; set; }

    public EventCheckServer()
    {
    }
}
