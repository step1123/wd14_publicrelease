using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.White.Cyborg.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cyborg.Systems;

public abstract class SharedCyborgSystemTransformAnimation : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgTransformAnimationComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CyborgTransformAnimationComponent, TransformAnimationEndEvent>(OnEnd);
        SubscribeLocalEvent<CyborgTransformAnimationComponent,ComponentHandleState>(OnHandle);
        SubscribeLocalEvent<CyborgTransformAnimationComponent,ComponentGetState>(OnGet);
    }

    private void OnEnd(EntityUid uid, CyborgTransformAnimationComponent component, TransformAnimationEndEvent args)
    {
        _actionBlocker.UpdateCanMove(uid);
    }

    private void OnGet(EntityUid uid, CyborgTransformAnimationComponent component,ref ComponentGetState args)
    {
        args.State = new CyborgTransformAnimationComponentState(component.Enabled);
    }

    private void OnHandle(EntityUid uid, CyborgTransformAnimationComponent component,ref ComponentHandleState args)
    {
        if(args.Current is not CyborgTransformAnimationComponentState state)
            return;

        component.Enabled = state.Enabled;
    }

    private void OnStartup(EntityUid uid, CyborgTransformAnimationComponent component, ComponentStartup args)
    {
        PlayAnimation(uid,component);
    }

    protected virtual void PlayAnimation(EntityUid uid, CyborgTransformAnimationComponent component,DoAfterComponent? doAfterComponent = null)
    {
        if(!component.Enabled || !Resolve(uid,ref doAfterComponent,false))
            return;

        var doAfterArgs = new DoAfterArgs(uid, component.Duration, new TransformAnimationEndEvent(),
            uid)
        {
            BreakOnHandChange = false,
            RequireCanInteract = false,
            BreakOnTargetMove = false,
            BreakOnUserMove = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);

        component.Enabled = false;
        Dirty(component);
    }
}

[Serializable, NetSerializable]
public sealed class TransformAnimationEndEvent : SimpleDoAfterEvent
{
}
