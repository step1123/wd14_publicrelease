using Content.Shared.Access;
using Content.Shared.FixedPoint;
using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.White.Cyborg.Components;

[RegisterComponent]
[NetworkedComponent]
public sealed class CyborgComponent : Component
{
    public const string ModuleContainerName = "module-slots";
    public const string BatterySlotId = "battery-slot";
    public const string InstrumentContainerName = "instrument-slots";

    [DataField("actions")] public List<ActionData> ActionsData = new();

    [ViewVariables] public bool Active = false;

    [ViewVariables] public ContainerSlot BatterySlot = default!;

    [DataField("consumption")] public FixedPoint2 Consumption = 0;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("drawRate")]
    public float DrawRate = 1f;

    [ViewVariables(VVAccess.ReadWrite)] public FixedPoint2 Energy = 0;

    [ViewVariables] public bool Freeze = false;

    [ViewVariables] public Container InstrumentContainer = default!;

    [ViewVariables] public List<EntityUid> InstrumentUids = new();

    [DataField("maxEnergy")] public FixedPoint2 MaxEnergy = 0;

    [DataField("moduleConsumption")] public FixedPoint2 ModuleConsumption = 0;

    [ViewVariables] public Container ModuleContainer = default!;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("moduleExtractionSound")]
    public SoundSpecifier ModuleExtractionSound = new SoundPathSpecifier("/Audio/Items/pistol_magout.ogg");

    [ViewVariables(VVAccess.ReadWrite)] [DataField("moduleInsertionSound")]
    public SoundSpecifier ModuleInsertionSound = new SoundPathSpecifier("/Audio/Items/pistol_magin.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keysExtractionMethod", customTypeSerializer: typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
    public string ModulesExtractionMethod = "Prying";

    [ViewVariables(VVAccess.ReadWrite)] [DataField("moduleSlots")]
    public int ModuleSlots = 6;

    [ViewVariables] public List<EntityUid> ModuleUids = new();

    [DataField("nextUpdate", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime;

    [DataField("panelLocked")] public bool PanelLocked = true;

    [DataField("prototype")] public string? Prototype;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("sparkSound")]
    public SoundSpecifier SparkSound = new SoundCollectionSpecifier("sparks");

    [DataField("unlockAccessTags", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<AccessLevelPrototype>))]
    public HashSet<string> UnlockAccessTags = new();
}

[DataDefinition]
[Serializable]
[NetSerializable]
public sealed class ActionData
{
    [DataField("important", true)] public bool Important;

    [DataField("name", true, required: true)]
    public string Name = default!;

    [DataField("key", true, required: true)]
    public Enum ActionKey { get; set; } = default!;
}

[Serializable]
[NetSerializable]
public sealed class CyborgComponentState : ComponentState
{
    public bool Active;
    public FixedPoint2 Consumption;
    public FixedPoint2 Energy;
    public List<EntityUid> InstrumentUids;
    public FixedPoint2 MaxEnergy;
    public bool PanelLocked;

    public CyborgComponentState(FixedPoint2 energy, FixedPoint2 maxEnergy, FixedPoint2 consumption,
        List<EntityUid> instrumentUids, bool active, bool panelLocked)
    {
        Energy = energy;
        MaxEnergy = maxEnergy;
        Consumption = consumption;
        InstrumentUids = instrumentUids;
        Active = active;
        PanelLocked = panelLocked;
    }
}
