using Robust.Shared.GameStates;
using Content.Server.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Animations;
using static Content.Shared.Animations.EmoteAnimationComponent;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;

namespace Content.Server.Animations;

public class EmoteAnimationSystem : SharedEmoteAnimationSystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<EmoteAnimationComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<EmoteAnimationComponent, MapInitEvent>(OnMapInint);
        SubscribeLocalEvent<EmoteAnimationComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<EmoteAnimationComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<EmoteAnimationComponent, EmoteFlipActionEvent>(OnEmoteFlip);
        SubscribeLocalEvent<EmoteAnimationComponent, EmoteJumpActionEvent>(OnEmoteJump);
        SubscribeLocalEvent<EmoteAnimationComponent, EmoteTurnActionEvent>(OnEmoteTurn);
    }

    private void OnGetState(EntityUid uid, EmoteAnimationComponent component, ref ComponentGetState args)
    {
        args.State = new EmoteAnimationComponentState(component.AnimationId);
    }

    private void OnMapInint(EntityUid uid, EmoteAnimationComponent component, MapInitEvent args)
    {

        var actionFlip = new InstantAction(_proto.Index<InstantActionPrototype>(EmoteFlipActionPrototype));
        var actionJump = new InstantAction(_proto.Index<InstantActionPrototype>(EmoteJumpActionPrototype));
        var actionTurn = new InstantAction(_proto.Index<InstantActionPrototype>(EmoteTurnActionPrototype));
        component.FlipAction = actionFlip;
        component.JumpAction = actionJump;
        component.TurnAction = actionTurn;
        _action.AddAction(uid, actionFlip, null);
        _action.AddAction(uid, actionJump, null);
        _action.AddAction(uid, actionTurn, null);
    }

    private void OnShutdown(EntityUid uid, EmoteAnimationComponent component, ComponentShutdown args)
    {
        if (component.FlipAction != null)
            _action.RemoveAction(uid, component.FlipAction);
        if (component.JumpAction != null)
            _action.RemoveAction(uid, component.JumpAction);
        if (component.TurnAction != null)
            _action.RemoveAction(uid, component.TurnAction);
    }

    private void OnEmote(EntityUid uid, EmoteAnimationComponent component, ref EmoteEvent args)
    {
        if (args.Handled || !args.Emote.Category.HasFlag(EmoteCategory.Gesture))
            return;

        PlayEmoteAnimation(uid, component, args.Emote.ID);
    }

    private void OnEmoteFlip(EntityUid uid, EmoteAnimationComponent component, EmoteFlipActionEvent args)
    {
        PlayEmoteAnimation(uid, component, EmoteFlipActionPrototype);
    }
    private void OnEmoteJump(EntityUid uid, EmoteAnimationComponent component, EmoteJumpActionEvent args)
    {
        PlayEmoteAnimation(uid, component, EmoteJumpActionPrototype);
    }
    private void OnEmoteTurn(EntityUid uid, EmoteAnimationComponent component, EmoteTurnActionEvent args)
    {
        PlayEmoteAnimation(uid, component, EmoteTurnActionPrototype);
    }

    public void PlayEmoteAnimation(EntityUid uid, EmoteAnimationComponent component, string emoteId)
    {
        component.AnimationId = emoteId;
        Dirty(component);
    }
}
