using Content.Server.EUI;
using Content.Server.Mind;
using Content.Server.Players;
using Content.Shared.Eui;
using Content.Shared.Ghost;
using Content.Shared.Tag;

namespace Content.Server.Ghost;

public sealed class ReturnToBodyEui : BaseEui
{
    private readonly MindSystem _mindSystem;
    private readonly TagSystem _tag; // WD

    private readonly Mind.Mind _mind;

    public ReturnToBodyEui(Mind.Mind mind, MindSystem mindSystem, TagSystem tagSystem) // WD EDIT
    {
        _mind = mind;
        _mindSystem = mindSystem;
        _tag = tagSystem; // WD
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not ReturnToBodyMessage choice ||
            !choice.Accepted)
        {
            Close();
            return;
        }

        if (_mind != null && _mindSystem.TryGetSession(_mind, out var session))
        // WD EDIT START
        {
            _mindSystem.UnVisit(session.ContentData()!.Mind);
            if (_mind.OwnedEntity.HasValue)
                _tag.AddTag(_mind.OwnedEntity!.Value, "DefibRevive");
        }
        // WD EDIT END
        Close();
    }
}
