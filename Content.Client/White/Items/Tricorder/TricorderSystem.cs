using Content.Client.Items;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared.White.Item.Tricorder;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Client.White.Items.Tricorder;

/// <inheritdoc/>
public sealed class TricorderSystem : SharedTricorderSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TricorderComponent, ItemStatusCollectMessage>(OnCollectItemStatus);
        SubscribeLocalEvent<TricorderComponent, ComponentHandleState>(HandleTricorderState);
    }

    private static void OnCollectItemStatus(EntityUid uid, TricorderComponent component, ItemStatusCollectMessage args)
    {
        if (component.CurrentMode != TricorderMode.Multitool)
        {
            args.Controls.Clear();
        }

        args.Controls.Add(new StatusControl(component));
    }

    private static void HandleTricorderState(EntityUid uid, TricorderComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not TricorderComponentState state)
        {
            return;
        }

        component.CurrentMode = state.CurrentMode;
    }

    private sealed class StatusControl : Control
    {
        private readonly RichTextLabel _label;
        private readonly TricorderComponent _tricorder;

        private TricorderMode? _linkModeActive;

        public StatusControl(TricorderComponent tricorder)
        {
            _tricorder = tricorder;
            _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
            AddChild(_label);
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            if (_linkModeActive != null && _linkModeActive == _tricorder.CurrentMode)
                return;

            _linkModeActive = _tricorder.CurrentMode;

            var modeLocString = GetNameByMode(_tricorder.CurrentMode);

            _label.SetMarkup(Robust.Shared.Localization.Loc.GetString("tricorder-item-status-label",
                ("mode", Robust.Shared.Localization.Loc.GetString(modeLocString))));
        }
    }
}