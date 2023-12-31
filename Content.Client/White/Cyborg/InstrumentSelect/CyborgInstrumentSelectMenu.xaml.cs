﻿using Content.Client.UserInterface.Controls;
using Content.Shared.White.Cyborg.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.White.Cyborg.InstrumentSelect;

[GenerateTypedNameReferences]
public sealed partial class CyborgInstrumentSelectMenu : FancyWindow
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public CyborgInstrumentSelectMenu(CyborgInstrumentSelectBoundInterface owner)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        Owner = owner;
    }

    public CyborgInstrumentSelectBoundInterface Owner { get; }

    public void UpdateInstruments(List<EntityUid> list)
    {
        InstrumentsList.DisposeAllChildren();
        list.Add(EntityUid.Invalid);
        foreach (var entity in list)
        {
            AppendInstrumentList(entity);
        }
    }

    public void UpdateEnergyPanel()
    {
        if (!_entityManager.TryGetComponent<CyborgComponent>(Owner.Machine, out var component))
            return;
        var en = component.Energy / component.MaxEnergy;
        EnergyBar.Value = en.Float();
        EnergyLabel.Text = Loc.GetString("examine-borg-energy", ("energy", component.Energy),
            ("maxEnergy", component.MaxEnergy));
    }

    public void AppendInstrumentList(EntityUid uid)
    {
        _entityManager.TryGetComponent<MetaDataComponent>(uid, out var metaDataComponent);
        var instrument = new CyborgInstrumentSelectContainer();
        if (metaDataComponent != null)
            instrument.SetLabel(metaDataComponent.EntityName);
        instrument.SetView(uid);
        instrument.OnButtonPressed(args =>
            Owner.SelectInstrument(uid)
        );

        InstrumentsList.AddChild(instrument);
    }
}
