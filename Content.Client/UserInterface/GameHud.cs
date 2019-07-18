using System;
using Content.Client.Utility;
using Robust.Client.Graphics;
using Robust.Client.Graphics.Drawing;
using Robust.Client.Interfaces.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface
{
    /// <summary>
    ///     Responsible for laying out the default game HUD.
    /// </summary>
    public interface IGameHud
    {
        Control RootControl { get; }

        // Escape top button.
        bool EscapeButtonDown { get; set; }
        Action<bool> EscapeButtonToggled { get; set; }

        // Character top button.
        bool CharacterButtonDown { get; set; }
        bool CharacterButtonVisible { get; set; }
        Action<bool> CharacterButtonToggled { get; set; }

        // Crafting top button.
        bool CraftingButtonDown { get; set; }
        bool CraftingButtonVisible { get; set; }
        Action<bool> CraftingButtonToggled { get; set; }

        // Init logic.
        void Initialize();
    }

    internal sealed class GameHud : IGameHud
    {
        private HBoxContainer _topButtonsContainer;
        private TopButton _buttonEscapeMenu;
        private TopButton _buttonTutorial;
        private TopButton _buttonCharacterMenu;
        private TopButton _buttonCraftingMenu;
        private TutorialWindow _tutorialWindow;

#pragma warning disable 649
        [Dependency] private readonly IResourceCache _resourceCache;
        [Dependency] private readonly ILocalizationManager _localizationManager;
#pragma warning restore 649

        public void Initialize()
        {
            RootControl = new Control {MouseFilter = Control.MouseFilterMode.Ignore};

            RootControl.SetAnchorPreset(Control.LayoutPreset.Wide);

            var escapeTexture = _resourceCache.GetTexture("/Textures/UserInterface/hamburger.svg.96dpi.png");
            var characterTexture = _resourceCache.GetTexture("/Textures/UserInterface/character.svg.96dpi.png");
            var craftingTexture = _resourceCache.GetTexture("/Textures/UserInterface/hammer.svg.96dpi.png");
            var tutorialTexture = _resourceCache.GetTexture("/Textures/UserInterface/students-cap.svg.96dpi.png");

            _topButtonsContainer = new HBoxContainer
            {
                SeparationOverride = 4
            };

            RootControl.AddChild(_topButtonsContainer);
            _topButtonsContainer.SetAnchorAndMarginPreset(Control.LayoutPreset.TopLeft, margin: 10);

            // TODO: Pull key names here from the actual key binding config.
            // Escape
            _buttonEscapeMenu = new TopButton(escapeTexture, "ESC")
            {
                ToolTip = _localizationManager.GetString("Open escape menu.")
            };

            _topButtonsContainer.AddChild(_buttonEscapeMenu);

            _buttonEscapeMenu.OnToggled += args => EscapeButtonToggled?.Invoke(args.Pressed);

            // Tutorial
            _buttonTutorial = new TopButton(tutorialTexture, " ")
            {
                ToolTip = _localizationManager.GetString("Open tutorial.")
            };

            _topButtonsContainer.AddChild(_buttonTutorial);

            _buttonTutorial.OnToggled += ButtonTutorialOnOnToggled;

            // Inventory
            _buttonCharacterMenu = new TopButton(characterTexture, "C")
            {
                ToolTip = _localizationManager.GetString("Open character menu."),
                Visible = false
            };

            _topButtonsContainer.AddChild(_buttonCharacterMenu);

            _buttonCharacterMenu.OnToggled += args => CharacterButtonToggled?.Invoke(args.Pressed);

            // Crafting
            _buttonCraftingMenu = new TopButton(craftingTexture, "G")
            {
                ToolTip = _localizationManager.GetString("Open crafting menu."),
                Visible = false
            };

            _topButtonsContainer.AddChild(_buttonCraftingMenu);

            _buttonCraftingMenu.OnToggled += args => CraftingButtonToggled?.Invoke(args.Pressed);

            _tutorialWindow = new TutorialWindow();

            _tutorialWindow.OnClose += () => _buttonTutorial.Pressed = false;
        }

        private void ButtonTutorialOnOnToggled(BaseButton.ButtonToggledEventArgs obj)
        {
            if (_tutorialWindow.IsOpen)
            {
                if (!_tutorialWindow.IsAtFront())
                {
                    _tutorialWindow.MoveToFront();
                }
                else
                {
                    _tutorialWindow.Close();
                }
            }
            else
            {
                _tutorialWindow.OpenCentered();
                _buttonTutorial.Pressed = true;
            }
        }

        public Control RootControl { get; private set; }

        public bool EscapeButtonDown
        {
            get => _buttonEscapeMenu.Pressed;
            set => _buttonEscapeMenu.Pressed = value;
        }

        public Action<bool> EscapeButtonToggled { get; set; }

        public bool CharacterButtonDown
        {
            get => _buttonCharacterMenu.Pressed;
            set => _buttonCharacterMenu.Pressed = value;
        }

        public bool CharacterButtonVisible
        {
            get => _buttonCharacterMenu.Visible;
            set => _buttonCharacterMenu.Visible = value;
        }

        public Action<bool> CharacterButtonToggled { get; set; }

        public bool CraftingButtonDown
        {
            get => _buttonCraftingMenu.Pressed;
            set => _buttonCraftingMenu.Pressed = value;
        }

        public bool CraftingButtonVisible
        {
            get => _buttonCraftingMenu.Visible;
            set => _buttonCraftingMenu.Visible = value;
        }

        public Action<bool> CraftingButtonToggled { get; set; }

        public sealed class TopButton : BaseButton
        {
            private static readonly Color ColorNormal = Color.FromHex("#7b7e9e");
            private static readonly Color ColorHovered = Color.FromHex("#9699bb");
            private static readonly Color ColorPressed = Color.FromHex("#00b061");

            private readonly VBoxContainer _container;
            private readonly TextureRect _textureRect;
            private readonly Label _label;

            public TopButton(Texture texture, string keyName)
            {
                ToggleMode = true;

                _container = new VBoxContainer {MouseFilter = MouseFilterMode.Ignore};
                AddChild(_container);
                _container.AddChild(_textureRect = new TextureRect
                {
                    Texture = texture,
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                    SizeFlagsVertical = SizeFlags.Expand | SizeFlags.ShrinkCenter,
                    MouseFilter = MouseFilterMode.Ignore,
                    ModulateSelfOverride = ColorNormal,
                    CustomMinimumSize = (0, 32),
                    Stretch = TextureRect.StretchMode.KeepCentered
                });

                _container.AddChild(_label = new Label
                {
                    Text = keyName,
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                    MouseFilter = MouseFilterMode.Ignore,
                    ModulateSelfOverride = ColorNormal
                });

                _container.SetAnchorAndMarginPreset(LayoutPreset.Wide);

                DrawModeChanged();
            }

            protected override Vector2 CalculateMinimumSize()
            {
                var styleSize = ActualStyleBox?.MinimumSize ?? Vector2.Zero;
                return (0, 4) + styleSize + _container?.CombinedMinimumSize ?? Vector2.Zero;
            }

            protected override void Draw(DrawingHandleScreen handle)
            {
                ActualStyleBox?.Draw(handle, PixelSizeBox);
            }

            private StyleBox ActualStyleBox
            {
                get
                {
                    TryGetStyleProperty(Button.StylePropertyStyleBox, out StyleBox ret);
                    return ret;
                }
            }

            protected override void DrawModeChanged()
            {
                switch (DrawMode)
                {
                    case DrawModeEnum.Normal:
                        StylePseudoClass = Button.StylePseudoClassNormal;
                        _textureRect.ModulateSelfOverride = ColorNormal;
                        _label.ModulateSelfOverride = ColorNormal;
                        break;

                    case DrawModeEnum.Pressed:
                        StylePseudoClass = Button.StylePseudoClassPressed;
                        _textureRect.ModulateSelfOverride = ColorPressed;
                        _label.ModulateSelfOverride = ColorPressed;
                        break;

                    case DrawModeEnum.Hover:
                        StylePseudoClass = Button.StylePseudoClassHover;
                        _textureRect.ModulateSelfOverride = ColorHovered;
                        _label.ModulateSelfOverride = ColorHovered;
                        break;

                    case DrawModeEnum.Disabled:
                        break;
                }
            }

            protected override void StylePropertiesChanged()
            {
                base.StylePropertiesChanged();

                if (_container == null)
                {
                    return;
                }

                var box = ActualStyleBox ?? new StyleBoxEmpty();

                _container.MarginLeft = box.GetContentMargin(StyleBox.Margin.Left);
                _container.MarginRight = -box.GetContentMargin(StyleBox.Margin.Right);
                _container.MarginTop = box.GetContentMargin(StyleBox.Margin.Top) + 4;
                _container.MarginBottom = -box.GetContentMargin(StyleBox.Margin.Bottom);
            }
        }
    }
}
