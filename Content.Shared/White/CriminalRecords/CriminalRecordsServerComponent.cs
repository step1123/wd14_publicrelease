using Content.Shared.StationRecords;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.CriminalRecords;

[Serializable, NetSerializable]
public enum EnumCriminalRecordType
{
    Released = 0,
    Discharged = 1,
    Parolled = 2,
    Suspected = 3,
    Wanted = 4,
    Incarcerated = 5
}

[Serializable, NetSerializable]
public sealed class CriminalRecordInfo
{

    // Main data
    [DataField("StationRecord")] public GeneralStationRecord StationRecord { get; set; }

    [DataField("CriminalType")] public EnumCriminalRecordType CriminalType { get; set; }
    [DataField("Reason")] public string Reason { get; set; }

    public CriminalRecordInfo(GeneralStationRecord stationRecord, EnumCriminalRecordType criminalType, string reason)
    {
        this.StationRecord = stationRecord;
        this.CriminalType = criminalType;
        this.Reason = reason;
    }
}

[RegisterComponent]
[NetworkedComponent]
public sealed class CriminalRecordsServerComponent : Component
{
    [DataField("Cache")] public Dictionary<StationRecordKey, CriminalRecordInfo> Cache = new();

    [Serializable, NetSerializable]
    public sealed class CriminalRecordsServerComponentState : ComponentState
    {
        public Dictionary<StationRecordKey, CriminalRecordInfo> Cache { get; init; }

        public CriminalRecordsServerComponentState(Dictionary<StationRecordKey, CriminalRecordInfo> cache)
        {
            Cache = cache;
        }
    }
}
