using Content.Server.GameTicking.Rules;
using Content.Server.Mind.Components;
using Content.Server.White;
using Content.Server.White.Cult.GameRule;
using Content.Server.White.Reva.Systems;
using Content.Server.Zombies;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private readonly ZombieSystem _zombie = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
    [Dependency] private readonly NukeopsRuleSystem _nukeopsRule = default!;
    [Dependency] private readonly PiratesRuleSystem _piratesRule = default!;
    [Dependency] private readonly CultRuleSystem _cultRuleSystem = default!;
    [Dependency] private readonly RevolutionaryRuleSystem _revaRuleSystem = default!;


    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!EntityManager.TryGetComponent<ActorComponent?>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        var targetHasMind = TryComp(args.Target, out MindContainerComponent? targetMindComp);
        if (!targetHasMind || targetMindComp == null)
            return;

        Verb traitor = new()
        {
            Text = "Make Traitor",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Structures/Wallmounts/posters.rsi"), "poster5_contraband"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _traitorRule.MakeTraitor(targetMindComp.Mind.Session);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-traitor"),
        };
        args.Verbs.Add(traitor);

        Verb zombie = new()
        {
            Text = "Make Zombie",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/Actions/zombie-turn.png")),
            Act = () =>
            {
                _zombie.ZombifyEntity(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-zombie"),
        };
        args.Verbs.Add(zombie);


        Verb nukeOp = new()
        {
            Text = "Make nuclear operative",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Structures/Wallmounts/signs.rsi"), "radiation"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _nukeopsRule.MakeLoneNukie(targetMindComp.Mind);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-nuclear-operative"),
        };
        args.Verbs.Add(nukeOp);

        Verb pirate = new()
        {
            Text = "Make Pirate",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/pirate.rsi"), "icon"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _piratesRule.MakePirate(targetMindComp.Mind);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-pirate"),
        };
        args.Verbs.Add(pirate);

        Verb cult = new()
        {
            Text = "Make Cultist",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/White/Cult/actions_cult.rsi"), "armor"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _cultRuleSystem.MakeCultist(targetMindComp.Mind.Session);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-cult"),
        };
        args.Verbs.Add(cult);

        Verb reva = new()
        {
            Text = "Make Head Reva",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/White/Overlays/Revolution/hud.rsi"), "reva_head"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _revaRuleSystem.MakeHeadRevolution(targetMindComp.Mind.Session);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-head-reva"),
        };
        args.Verbs.Add(reva);
    }
}
