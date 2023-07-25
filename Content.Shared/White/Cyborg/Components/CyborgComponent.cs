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


[RegisterComponent, NetworkedComponent]
public sealed class CyborgComponent : Component
{
    [ViewVariables]
    public bool Active = false;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("moduleSlots")]
    public int ModuleSlots = 6;

    [ViewVariables]
    public Container ModuleContainer = default!;
    public const string ModuleContainerName = "module-slots";
    [ViewVariables]
    public List<EntityUid> ModuleUids = new();

    [ViewVariables]
    public ContainerSlot BatterySlot = default!;
    public const string BatterySlotId = "battery-slot";

    [ViewVariables]
    public ContainerSlot BrainSlot = default!;
    public const string BrainSlotId = "brain-slot";
    public EntityUid? BrainUid;

    [ViewVariables]
    public Container InstrumentContainer = default!;
    public const string InstrumentContainerName = "instrument-slots";

    [ViewVariables]
    public List<EntityUid> InstrumentUids = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Energy = 0;

    [DataField("maxEnergy")]
    public FixedPoint2 MaxEnergy = 0;

    [DataField("consumption")]
    public FixedPoint2 Consumption = 0;

    [DataField("moduleConsumption")]
    public FixedPoint2 ModuleConsumption = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("drawRate")]
    public float DrawRate = 1f;

    [DataField("nextUpdate", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime;

    [DataField("panelLocked")]
    public bool PanelLocked = true;

    [DataField("unlockAccessTags", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<AccessLevelPrototype>))]
    public HashSet<string> UnlockAccessTags = new();

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keysExtractionMethod", customTypeSerializer: typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
    public string ModulesExtractionMethod = "Prying";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("moduleExtractionSound")]
    public SoundSpecifier ModuleExtractionSound = new SoundPathSpecifier("/Audio/Items/pistol_magout.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("moduleInsertionSound")]
    public SoundSpecifier ModuleInsertionSound = new SoundPathSpecifier("/Audio/Items/pistol_magin.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("sparkSound")]
    public SoundSpecifier SparkSound = new SoundCollectionSpecifier("sparks");

    [DataField("prototype")] public string? Prototype;

    [DataField("actions")]
    public List<ActionData> ActionsData = new();

    [ViewVariables] public bool Freeze = false;
}

[DataDefinition,Serializable,NetSerializable]
public sealed class ActionData
{
    [DataField("name", readOnly: true, required: true)]
    public string Name = default!;

    [DataField("key", readOnly: true, required: true)]
    public Enum ActionKey { get; set; } = default!;

    [DataField("important", readOnly: true)]
    public bool Important = false;
}


[Serializable,NetSerializable]
public sealed class CyborgComponentState : ComponentState
{
    public bool Active;
    public bool PanelLocked;
    public FixedPoint2 Energy;
    public FixedPoint2 MaxEnergy;
    public FixedPoint2 Consumption;
    public List<EntityUid> InstrumentUids;

    public CyborgComponentState(FixedPoint2 energy, FixedPoint2 maxEnergy, FixedPoint2 consumption,
        List<EntityUid> instrumentUids,bool active,bool panelLocked)
    {
        Energy = energy;
        MaxEnergy = maxEnergy;
        Consumption = consumption;
        InstrumentUids = instrumentUids;
        Active = active;
        PanelLocked = panelLocked;
    }
}

