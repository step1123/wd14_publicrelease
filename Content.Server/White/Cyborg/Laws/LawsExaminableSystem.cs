using Content.Shared.Examine;
using Content.Shared.Verbs;
using Content.Shared.White.Cyborg.Laws.Component;
using Robust.Shared.Utility;

namespace Content.Server.White.Cyborg.Laws;

public sealed class LawsExaminableSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LawsComponent,GetVerbsEvent<ExamineVerb>>(OnExamine);
    }

    private void OnExamine(EntityUid uid, LawsComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if(!HasComp<LawsExaminerComponent>(args.User))
            return;

        var detailsRange = _examineSystem.IsInDetailsRange(args.User, uid);

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var markup = CreateMarkup(uid, component);
                _examineSystem.SendExamineTooltip(args.User, uid, markup, false, false);
            },
            Text = Loc.GetString("laws-examinable-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = Loc.GetString("laws-examinable-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    public FormattedMessage CreateMarkup(EntityUid uid, LawsComponent component)
    {
        var msg = new FormattedMessage();

        foreach (var law in component.Laws)
        {
            msg.PushNewline();
            msg.AddText($"- {law}");
            msg.PushNewline();
        }

        return msg;
    }
}
