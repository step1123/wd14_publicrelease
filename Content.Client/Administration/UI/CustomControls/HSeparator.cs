﻿using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client.Administration.UI.CustomControls;

public class HSeparator : Control
{
    private static readonly Color SeparatorColor = Color.FromHex("#3D4059");

    public HSeparator(Color color)
    {
        AddChild(new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = color,
                ContentMarginBottomOverride = 2, ContentMarginLeftOverride = 2
            }
        });
    }

    public HSeparator() : this(SeparatorColor) { }
}
