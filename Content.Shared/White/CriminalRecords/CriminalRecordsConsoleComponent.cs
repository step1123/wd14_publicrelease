using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Radio;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.CriminalRecords;

[RegisterComponent]
public sealed class CriminalRecordsConsoleComponent : Component
{
    public const string IdSlotId = "id-slot";

    [DataField("idSlot")]
    public ItemSlot IdSlot = new();

    [DataField("TargetChannel", customTypeSerializer: typeof(PrototypeIdSerializer<RadioChannelPrototype>))]
    public string TargetChannel = "Security";

    [ViewVariables] public IdCardComponent? ContainedID;
    [ViewVariables] public StationRecordKey? ActiveKey { get; set; }
}

[Serializable, NetSerializable]
public sealed class IdCardNetInfo
{
    public string? FullName { get; }
    public string? JobTitle { get; }

    public IdCardNetInfo(string? fullName, string? jobTitle)
    {
        FullName = fullName;
        JobTitle = jobTitle;
    }
}

[Serializable, NetSerializable]
public sealed class CriminalRecordsConsoleBuiState : BoundUserInterfaceState
{
    /// <summary>
    ///     Current selected key.
    /// </summary>
    public StationRecordKey? SelectedKey { get; }
    public GeneralStationRecord? Record { get; }
    public Dictionary<StationRecordKey, string>? RecordListing { get; }
    public Dictionary<StationRecordKey, CriminalRecordInfo>? Cache { get; }
    public IdCardNetInfo? ContainedId { get; }
    public bool IsAllowed { get; }
    //public GeneralStationRecordsFilter? Filter { get; }
    public CriminalRecordsConsoleBuiState(StationRecordKey? key, GeneralStationRecord? record,
        Dictionary<StationRecordKey, string>? recordListing, Dictionary<StationRecordKey, CriminalRecordInfo>? cache
           , IdCardNetInfo? containedId, bool isAllowed) //GeneralStationRecordsFilter? newFilter
    {
        SelectedKey = key;
        Record = record;
        RecordListing = recordListing;
        Cache = cache;
        ContainedId = containedId;
        IsAllowed = isAllowed;
        //Filter = newFilter;
    }

    public bool IsEmpty() => SelectedKey == null
                             && Record == null && RecordListing == null;
}

[Serializable, NetSerializable]
public sealed class SelectCriminalRecord : BoundUserInterfaceMessage
{
    public StationRecordKey? SelectedKey { get; }
//
    public SelectCriminalRecord(StationRecordKey? selectedKey)
    {
        SelectedKey = selectedKey;
    }
}

[Serializable, NetSerializable]
public sealed class SelectCriminalStatus : BoundUserInterfaceMessage
{
    public StationRecordKey SelectedKey { get; }
    public CriminalRecordInfo? SelectedStatus { get; }

    public SelectCriminalStatus(StationRecordKey selectedKey, CriminalRecordInfo? selectedStatus)
    {
        SelectedKey = selectedKey;
        SelectedStatus = selectedStatus;
    }
}

[Serializable, NetSerializable]
public sealed class SelectCriminalReason : BoundUserInterfaceMessage
{
    public StationRecordKey SelectedKey { get; }
    public string Text { get; }

    public SelectCriminalReason(StationRecordKey key, string text)
    {
        SelectedKey = key;
        Text = text;
    }
}

[Serializable, NetSerializable]
public enum CriminalRecordsConsoleKey : byte
{
    Key
}
