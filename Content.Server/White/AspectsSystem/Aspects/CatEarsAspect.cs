using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Speech.Components;
using Content.Server.White.AspectsSystem.Aspects.Components;
using Content.Server.White.AspectsSystem.Base;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Speech;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.White.AspectsSystem.Aspects;

public sealed class CatEarsAspect : AspectSystem<CatEarsAspectComponent>
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    private MarkingPrototype _ears = default!;
    private MarkingPrototype _tail = default!;

    private const string FemaleFelinidVoices = "FemaleFelinid";
    private const string MaleFelinidVoices = "MaleFelinid";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnd);

        _ears = _protoMan.Index<MarkingPrototype>("FelinidEarsBasic");
        _tail = _protoMan.Index<MarkingPrototype>("FelinidTailBasic");
    }

    private void OnRoundEnd(RoundEndedEvent ev)
    {
        var query = EntityQueryEnumerator<CatEarsAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            var entQuery = EntityQueryEnumerator<SpeechComponent, HumanoidAppearanceComponent>();
            while (entQuery.MoveNext(out var ent, out _, out _))
            {
                _chat.TrySendInGameICMessage(ent, _random.Pick(new[] { "Мяу", "Мур", "Ня" }), InGameICChatType.Speak,
                    ChatTransmitRange.Normal);
            }
        }
    }

    protected override void Started(
        EntityUid uid,
        CatEarsAspectComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<HumanoidAppearanceComponent>();
        while (query.MoveNext(out var ent, out var appearance))
        {
            AddMarkings(ent, appearance);
        }
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<CatEarsAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!ev.LateJoin)
                return;

            AddMarkings(ev.Mob);
        }
    }

    private void AddMarkings(EntityUid uid, HumanoidAppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref appearance, false))
            return;

        switch (appearance.Species)
        {
            case "Felinid":
                return;
            case "Human":
            {
                if (!appearance.MarkingSet.TryGetCategory(MarkingCategories.HeadTop, out var markings) ||
                    markings.Count == 0)
                    AddEars(appearance);

                if (!appearance.MarkingSet.TryGetCategory(MarkingCategories.Tail, out markings) || markings.Count == 0)
                    AddTail(appearance);

                Dirty(appearance);
                ChangeEmotesVoice(uid, appearance);
                return;
            }
            default:
                AddEars(appearance);
                AddTail(appearance);
                Dirty(appearance);
                ChangeEmotesVoice(uid, appearance);
                break;
        }
    }

    private List<Color> GetColors(HumanoidAppearanceComponent appearance, MarkingPrototype prototype)
    {
        return MarkingColoring.GetMarkingLayerColors(prototype, appearance.SkinColor, appearance.EyeColor,
            appearance.MarkingSet);
    }

    private void AddTail(HumanoidAppearanceComponent appearance)
    {
        if (!appearance.MarkingSet.TryGetMarking(MarkingCategories.Tail, _tail.ID, out _))
        {
            appearance.MarkingSet.AddFront(MarkingCategories.Tail,
                new Marking(_tail.ID, GetColors(appearance, _tail)) { Forced = true });
        }
    }

    private void AddEars(HumanoidAppearanceComponent appearance)
    {
        if (!appearance.MarkingSet.TryGetMarking(MarkingCategories.HeadTop, _tail.ID, out _))
        {
            appearance.MarkingSet.AddFront(MarkingCategories.HeadTop,
                new Marking(_ears.ID, GetColors(appearance, _ears)) { Forced = true });
        }
    }

    private void ChangeEmotesVoice(EntityUid user, HumanoidAppearanceComponent appearanceComponent)
    {
        if (!TryComp(user, out VocalComponent? vocals))
        {
            return;
        }

        switch (appearanceComponent.Gender)
        {
            case Gender.Female:
                _protoMan.TryIndex(FemaleFelinidVoices, out vocals.EmoteSounds);
                break;
            case Gender.Male:
                _protoMan.TryIndex(MaleFelinidVoices, out vocals.EmoteSounds);
                break;
        }
    }
}