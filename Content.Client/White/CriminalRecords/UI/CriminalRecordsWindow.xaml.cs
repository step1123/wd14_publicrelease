using System.Linq;
using Content.Client.Humanoid;
using Content.Client.Inventory;
using Content.Client.Resources;
using Content.Client.White.CriminalRecords.UI.Controls;
using Content.Shared.Access.Systems;
using Content.Shared.CrewManifest;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Radio.Components;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Content.Shared.White.CriminalRecords;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Client.Utility;
using Robust.Shared.Console;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.White.CriminalRecords.UI;

[GenerateTypedNameReferences]
public sealed partial class CriminalRecordsWindow : DefaultWindow
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public Action<StationRecordKey>? OnKeySelected;
    public Action<StationRecordKey, string>? OnTextBindDown;
    public Action<StationRecordKey, CriminalRecordInfo>? OnStatusSelected;

    public CriminalRecordsWindow() : base()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
    }

    public void UpdateState(CriminalRecordsConsoleBuiState state)
    {
        // Check access AllAccess Security
        var jobs = _prototypeManager.EnumeratePrototypes<JobPrototype>();
        var canAccess = state.IsAllowed;
        // Check if exists card or not
        if (state.ContainedId == null || !canAccess)
        {
            var messageHint = new FormattedMessage();
            messageHint.AddMarkup(Loc.GetString("criminal-login-hint", ("name", Loc.GetString("criminal-login-in"))));
            messageHint.AddMarkup("\n\n");
            messageHint.AddMarkup(Loc.GetString("criminal-login-warn"));
            LoginHint.SetMessage(messageHint);
            MainContent.Visible = false;
            NonServerContent.Visible = false;
            NonAccessContent.Visible = true;
            return;
        }

        if (!state.HasServer)
        {
            var shader = _prototypeManager.Index<ShaderPrototype>("CameraStatic").Instance().Duplicate();
            NoiseBackground.Texture = _resourceCache.GetTexture("/Textures/Interface/Nano/square_black.png");
            NoiseBackground.ShaderOverride = shader;

            NonUserLabel.SetMessage(Loc.GetString("criminal-login-info",
                ("user", (state.ContainedId.FullName ?? string.Empty) + ", " +
                         (state.ContainedId.JobTitle ?? string.Empty) )));

            MainContent.Visible = false;
            NonServerContent.Visible = true;
            NonAccessContent.Visible = false;
            return;
        }

        MainContent.Visible = true;
        NonServerContent.Visible = false;
        NonAccessContent.Visible = false;

        // Init header panel
        UserLabel.SetMessage(Loc.GetString("criminal-login-info",
            ("user", (state.ContainedId.FullName ?? string.Empty) + ", " +
                     (state.ContainedId.JobTitle ?? string.Empty) )));

        // Make crew list
        Populate(state, state.RecordListing);

        // Make card
        if (state is { SelectedKey: not null, Record: not null })
        {
            CreateRecordCard(state, state.SelectedKey.Value, state.Record);
        }
    }

    public void Populate(CriminalRecordsConsoleBuiState State, Dictionary<StationRecordKey, string>? RecordListing)
    {
        if (RecordListing == null)
            return;

        // clear govno from list
        RecordsListContainer.RemoveAllChildren();

        foreach (var (recordKey, name) in RecordListing)
        {
            var element = CreateRecordItem(State, recordKey, name);
            element.ButtonElement.ToolTip = Loc.GetString("criminal-list-focus");
            element.ButtonElement.OnPressed += _ =>
            {
                OnKeySelected?.Invoke(recordKey);
            };
        }
    }

    private CriminalRecordInfo? GetRecord(StationRecordKey Key, Dictionary<StationRecordKey, CriminalRecordInfo> Cache)
    {
        foreach (var (key, info) in Cache)
        {
            if (Key.ID == key.ID)
            {
                return info;
            }
        }
        return null;
    }

    private Color GetColor(StationRecordKey Key, Dictionary<StationRecordKey, CriminalRecordInfo> Cache)
    {
        var info = GetRecord(Key, Cache);
        if (info == null)
            return new Color(0,0,0);

        switch (info.CriminalType)
        {
            case EnumCriminalRecordType.Released:
                return new Color(14, 106, 254);
            case EnumCriminalRecordType.Discharged:
                return new Color(14,106,254);
            case EnumCriminalRecordType.Parolled:
                return new Color(151,196,66);
            case EnumCriminalRecordType.Suspected:
                return new Color(217,126,35);
            case EnumCriminalRecordType.Wanted:
                return new Color(190, 50 ,50);
            case EnumCriminalRecordType.Incarcerated:
                return new Color(196,164,114);
        }
        return new Color(0,0,0);
    }

    private RecordItem CreateRecordItem(CriminalRecordsConsoleBuiState State, StationRecordKey Key, string Name)
    {
        var record = new RecordItem();
        record.VerticalAlignment = Control.VAlignment.Top;
        if (State.Cache != null && GetRecord(Key, State.Cache) != null)
        {
            record.SideLineElement.ModulateSelfOverride = GetColor(Key, State.Cache);
        }
        else
        {
            record.SideLineElement.ModulateSelfOverride = new Color(0, 0 ,0);
        }
        record.NameLabel.SetMessage(Name);
        // append element into list
        RecordsListContainer.AddChild(record);
        // result
        return record;
    }

    private RecordCard CreateRecordCard(CriminalRecordsConsoleBuiState State, StationRecordKey Key, GeneralStationRecord Record)
    {
        var card = new RecordCard();
        // set color
        if (State.Cache != null && GetRecord(Key, State.Cache) != null)
        {
            card.SideLineElement.ModulateSelfOverride = GetColor(Key, State.Cache);
        }
        else
        {
            card.SideLineElement.ModulateSelfOverride = new Color(0, 0 ,0);
        }
        // name
        card.CharacterNameLabel.Text = Record.Name + ", " + Record.JobTitle;
        // job icon
        var path = new ResPath("/Textures/Interface/Misc/job_icons.rsi");
        _resourceCache.TryGetResource(path, out RSIResource? rsi);

        if (rsi != null)
        {
            if (rsi.RSI.TryGetState(Record.JobIcon, out _))
            {
                var specifier = new SpriteSpecifier.Rsi(path, Record.JobIcon);
                card.JobIcon.Texture = specifier.Frame0();
            }
            else if (rsi.RSI.TryGetState("Unknown", out _))
            {
                var specifier = new SpriteSpecifier.Rsi(path, "Unknown");
                card.JobIcon.Texture = specifier.Frame0();
            }
        }
        // status icon
        card.ViewIcon.Sprite = CreateCharacterDummy(Record);
        // info
        var dnaInfo = "";
        if (Record.DNA != null)
            dnaInfo = Record.DNA;

        var fingerprintInfo = "";
        if (Record.Fingerprint != null)
            fingerprintInfo = Record.Fingerprint;

        var message = new FormattedMessage();
        message.AddMarkup(Loc.GetString("criminal-dna-name"));
        message.AddMarkup("\n");
        message.AddMarkup(Loc.GetString("criminal-dna-desc",
            ("color", new Color(171,129,222).ToHex()), ("info", dnaInfo) ));
        message.AddMarkup("\n");
        message.AddMarkup(Loc.GetString("criminal-fingerprint-name"));
        message.AddMarkup("\n");
        message.AddMarkup(Loc.GetString("criminal-fingerprint-desc",
            ("color", new Color(171,129,222).ToHex()), ("info", fingerprintInfo) ));
        card.DetailLabel.SetMessage(message);
        // clear and aapend
        RecordCardContainer.DisposeAllChildren();
        RecordCardContainer.AddChild(card);
        // some shit (like change status)
        card.StatusOption.OnItemSelected += eventArgs =>
        {
            if (!card._canDoEvent)
                return;

            var record = new CriminalRecordInfo(Record, (EnumCriminalRecordType)eventArgs.Id, card.ReasonWritten.GetMessage() ?? string.Empty);
            OnStatusSelected?.Invoke(Key, record);
            card.StatusOption.SelectId(eventArgs.Id);
        };
        // init status option
        if (State.Cache != null)
        {
            card.InitializeStatusOption(Key, State.Cache);
            card._canDoEvent = true;
        }
        // init reason text
        if (State.Cache != null)
        {
            var criminalInfo = GetRecord(Key, State.Cache);
            if (criminalInfo != null)
                card.ReasonWritten.SetMessage(criminalInfo.Reason);
        }

        card.OnTextBindDown += args =>
        {
            OnTextBindDown?.Invoke(Key, args);
        };
        // rsult
        return card;
    }

    private SpriteComponent? CreateCharacterDummy(GeneralStationRecord Record)
    {
        IEntityManager entityManager = IoCManager.Resolve<IEntityManager>();
        IPrototypeManager prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        HumanoidAppearanceSystem appearanceSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<HumanoidAppearanceSystem>();

        var profile = Record.Profile ?? new HumanoidCharacterProfile();
        var _previewDummy = entityManager.SpawnEntity(prototypeManager.Index<SpeciesPrototype>(profile.Species).DollPrototype, MapCoordinates.Nullspace);
        appearanceSystem.LoadProfile(_previewDummy, profile);
        GiveDummyJobClothes(_previewDummy, Record.JobPrototype, profile);

        var sprite = entityManager.GetComponent<SpriteComponent>(_previewDummy);
        return sprite;
    }

    private void GiveDummyJobClothes(EntityUid dummy, string jobPrototype, HumanoidCharacterProfile profile)
    {
        IEntityManager entityManager = IoCManager.Resolve<IEntityManager>();
        IPrototypeManager prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        ClientInventorySystem inventorySystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ClientInventorySystem>();

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract (what is resharper smoking?)
        var job = prototypeManager.Index<JobPrototype>(jobPrototype ?? SharedGameTicker.FallbackOverflowJob);

        if (job.StartingGear != null && inventorySystem.TryGetSlots(dummy, out var slots))
        {
            var gear = prototypeManager.Index<StartingGearPrototype>(job.StartingGear);

            foreach (var slot in slots)
            {
                var itemType = gear.GetGear(slot.Name, profile);
                if (inventorySystem.TryUnequip(dummy, slot.Name, out var unequippedItem, true, true))
                {
                    entityManager.DeleteEntity(unequippedItem.Value);
                }

                if (itemType != string.Empty)
                {
                    var item = entityManager.SpawnEntity(itemType, MapCoordinates.Nullspace);
                    inventorySystem.TryEquip(dummy, item, slot.Name, true, true);
                }
            }
        }
    }

    private void GetAccess()
    {
        //_accessReader.FindAccessTags(item).ToArray();
    }
}
