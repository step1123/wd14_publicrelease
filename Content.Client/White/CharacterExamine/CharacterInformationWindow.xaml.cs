using System.Linq;
using Content.Client.Message;
using Content.Client.UserInterface.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.White.CharacterExamine;

[GenerateTypedNameReferences]
public sealed partial class CharacterInformationWindow : FancyWindow
{
    private readonly IEntityManager _entity;

    // ReSharper disable once InconsistentNaming
    private GridContainer _sprites => SpriteContainer;
    // ReSharper disable once InconsistentNaming
    private RichTextLabel _name => Name;
    // ReSharper disable once InconsistentNaming
    private RichTextLabel _job => Job;
    // ReSharper disable once InconsistentNaming
    private RichTextLabel _flavor => FlavorText;

    public CharacterInformationWindow()
    {
        RobustXamlLoader.Load(this);

        _entity = IoCManager.Resolve<IEntityManager>();

        ResetUi();
    }


    /// <summary>
    ///     Placeholder entries
    /// </summary>
    public void ResetUi()
    {
        _sprites.RemoveAllChildren();

        var unknown = Loc.GetString("generic-unknown");
        // Capitalize the first letter of each word (Title Case)
        unknown = string.Join(" ", unknown.Split(' ').Select(s => char.ToUpper(s[0]) + s[1..]));

        _name.SetMarkup(unknown);
        _job.SetMarkup(unknown);

        _flavor.SetMarkup(Loc.GetString("character-information-ui-flavor-text-placeholder"));
    }

    /// <summary>
    ///     Updates the UI to show all relevant information about the entity
    /// </summary>
    /// <param name="examined">The entity to become informed about</param>
    /// <param name="name">The name of the examined entity, taken from their ID</param>
    /// <param name="job">The job of the examined entity, taken from their ID</param>
    /// <param name="flavorText">The flavor text of the examined entity</param>
    public void UpdateUi(EntityUid examined, string? name = null, string? job = null, string? flavorText = null)
    {
        ResetUi();

        // Fill in the omnidirectional sprite views
        if (_entity.TryGetComponent<SpriteComponent>(examined, out var sprite))
        {
            FillSprites(sprite);
        }

        // Fill in the name and job
        if (!string.IsNullOrEmpty(name))
            _name.SetMarkup(name);
        if (!string.IsNullOrEmpty(job))
            _job.SetMarkup(job);

        // Fill in the flavor text
        if (!string.IsNullOrEmpty(flavorText))
            _flavor.SetMessage(flavorText);
    }


    /// <summary>
    ///     Fills the sprite views with the sprite from the sprite component
    /// </summary>
    /// <remarks>
    ///     Stupid, redefines the sprite view 4 times, can't find another way to do this
    /// </remarks>
    /// <param name="sprite">Sprite component to use</param>
    private void FillSprites(SpriteComponent sprite)
    {
        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.South,
            Margin = new Thickness(0, 0, 8, 8),
        });

        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.North,
            Margin = new Thickness(8, 0, 0, 8),
        });

        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.West,
            Margin = new Thickness(0, 8, 8, 0),
        });

        _sprites.AddChild(new SpriteView
        {
            Sprite = sprite,
            Scale = (4, 4),
            OverrideDirection = Direction.East,
            Margin = new Thickness(8, 8, 0, 0),
        });
    }
}
