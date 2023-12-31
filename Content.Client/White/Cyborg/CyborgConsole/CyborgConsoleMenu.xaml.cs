﻿using Content.Client.UserInterface.Controls;
using Content.Shared.White.Cyborg;
using Content.Shared.White.Cyborg.CyborgSensor;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Cyborg.CyborgConsole;

[GenerateTypedNameReferences]
public sealed partial class CyborgConsoleMenu : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    public string? LastAddress;
    public Dictionary<string, CyborgSensorStatus> Sensors = new();

    public CyborgConsoleMenu(CyborgConsoleBoundUserInterface owner)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        Owner = owner;
    }

    public CyborgConsoleBoundUserInterface Owner { get; }

    public void UpdateCyborgsList(List<CyborgSensorStatus> sensors)
    {
        CyborgList.DisposeAllChildren();
        Sensors.Clear();
        foreach (var sensor in sensors)
        {
            Sensors.Add(sensor.Address, sensor);
            AddOnCyborgList(sensor.Address);
        }

        UpdateUnitInformation(LastAddress);
    }

    public void AddOnCyborgList(string address)
    {
        if (!Sensors.TryGetValue(address, out var sensor))
            return;

        var cyborg = new CyborgConsoleUnitContainer();
        cyborg.SetTitle(sensor.Name);
        cyborg.OnButtonPressed(args =>
        {
            CyborgInformation.DisposeAllChildren();
            UpdateUnitInformation(address);
        });

        if (sensor.Prototype != null &&
            _prototypeManager.TryIndex<CyborgPrototype>(sensor.Prototype, out var prototype))
            cyborg.SetView(prototype.Icon);

        CyborgList.AddChild(cyborg);
    }

    public void UpdateUnitInformation(string? address)
    {
        if (CyborgInformation.ChildCount > 0 &&
            CyborgInformation.GetChild(0) is not CyborgConsoleUnitInformationContainer)
            return;

        CyborgInformation.DisposeAllChildren();
        if (address == null || !Sensors.TryGetValue(address, out var sensor))
            return;

        LastAddress = address;

        var unitInformation = new CyborgConsoleUnitInformationContainer();
        unitInformation.SetName(sensor.Name);
        unitInformation.SetEnergyInfo(Loc.GetString("examine-borg-energy-percent",
            ("energy", (int) (sensor.Energy / sensor.MaxEnergy * 100))));
        unitInformation.SetEnergyBar(sensor.Energy.Float(), sensor.MaxEnergy.Float());
        unitInformation.SetCyborgActiveInfo(sensor.IsActive);
        unitInformation.SetCyborgPanelLockedInfo(sensor.IsPanelLocked);
        unitInformation.SetCyborgAlive(sensor.IsAlive);
        unitInformation.SetCyborgFreeze(sensor.Freeze);

        if (sensor.АvailableAction != null)
        {
            foreach (var action in sensor.АvailableAction)
            {
                unitInformation.AddButton(action, args =>
                    Owner.SendActionMessage(sensor.Address, action.ActionKey)
                );
            }
        }

        if (sensor.Prototype != null &&
            _prototypeManager.TryIndex<CyborgPrototype>(sensor.Prototype, out var prototype))
            unitInformation.SetView(prototype.Icon);

        SetUnitInformation(unitInformation);
    }

    public void SetUnitInformation(PanelContainer container)
    {
        CyborgInformation.DisposeAllChildren();
        CyborgInformation.AddChild(container);
    }
}
