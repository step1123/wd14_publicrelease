using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Tiles;
using Content.Shared.Wieldable;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Medical.BodyScanner
{
    [Serializable, NetSerializable]
    public enum BodyScannerConsoleUIKey : byte
    {
        Key
    }

    [Serializable, NetSerializable]
    public sealed class BodyScannerConsoleBoundUserInterfaceState : BoundUserInterfaceState
    {
        public bool ScannerConnected;
        public bool GeneticScannerInRange;
        public bool CanScan;
        public bool CanPrint;

        public bool Scanning;
        public TimeSpan ScanTimeRemaining;
        public TimeSpan ScanTotalTime;

        public EntityUid? TargetEntityUid;
        public string EntityName;

        public float BleedAmount;
        public FixedPoint2 BloodMaxVolume;
        public string BloodReagent;
        public Solution BloodSolution;
        public FixedPoint2 ChemicalMaxVolume;
        public Solution ChemicalSolution;

        public string DNA;
        public string Fingerprint;

        public bool HasMind;

        public float MaxSaturation;
        public float MinSaturation;
        public float Saturation;

        public float HeatDamageThreshold;
        public float ColdDamageThreshold;
        public float CurrentTemperature;

        public float CurrentThirst;
        public byte CurrentThirstThreshold;

        public FixedPoint2 TotalDamage;
        public Dictionary<string, FixedPoint2> DamageDict;
        public Dictionary<string, FixedPoint2> DamagePerGroup;
        public FixedPoint2 DeadThreshold;

        public float CurrentHunger;
        public byte CurrentHungerThreshold;

        public MobState CurrentState;

        public float StaminaCritThreshold;
        public float StaminaDamage;

        public BodyScannerConsoleBoundUserInterfaceState()
        {
            CanScan = true;
            EntityName = "N/A";
            BloodMaxVolume = FixedPoint2.Zero;
            BloodReagent = "N/A";
            ChemicalMaxVolume = FixedPoint2.Zero;
            BloodSolution = new();
            ChemicalSolution = new();
            DNA = "";
            Fingerprint = "";
            TotalDamage = FixedPoint2.Zero;
            DamageDict = new();
            DamagePerGroup = new();
            DeadThreshold = FixedPoint2.Zero;
            CurrentState = MobState.Invalid;
        }
    }

    [Serializable, NetSerializable]
    public sealed class BodyScannerStartScanningMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class BodyScannerStartPrintingMessage : BoundUserInterfaceMessage
    {
    }
}
