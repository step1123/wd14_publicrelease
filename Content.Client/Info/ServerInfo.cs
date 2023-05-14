using Content.Client.Changelog;
using Content.Client.Credits;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

// WD-EDIT start
namespace Content.Client.Info
{
    public sealed class ServerInfo : BoxContainer
    {
        private readonly RichTextLabel _richTextLabelLeft;
        private readonly RichTextLabel _richTextLabelRight;
        private readonly RichTextLabel _richTextLabelGayNigger;

        public ServerInfo()
        {
            Orientation = LayoutOrientation.Vertical;

            var whatTheFuckImActuallyDoing = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalAlignment = HAlignment.Left,
                HorizontalExpand = true
            };

            _richTextLabelLeft = new RichTextLabel
            {
                MinWidth = 200
            };
            _richTextLabelRight = new RichTextLabel
            {
                VerticalAlignment = VAlignment.Top
            };
            _richTextLabelGayNigger = new RichTextLabel
            {
                HorizontalAlignment = HAlignment.Left,
                MaxWidth = 500
            };
            AddChild(whatTheFuckImActuallyDoing);
            whatTheFuckImActuallyDoing.AddChild(_richTextLabelLeft);
            whatTheFuckImActuallyDoing.AddChild(_richTextLabelRight);
            AddChild(_richTextLabelGayNigger);
        }
        public void SetInfoBlob(string markup) // мне похуй, поебать абсолютно, я ненавижу этот блядский язык, поймите
        {
            var yaica = markup.Split("###");
            _richTextLabelLeft.SetMessage(FormattedMessage.FromMarkup(yaica[0]));
            _richTextLabelRight.SetMessage(FormattedMessage.FromMarkup(yaica[1]));
            _richTextLabelGayNigger.SetMessage(FormattedMessage.FromMarkup(yaica[2]));
        }
    }
}
// WD-EDIT end
