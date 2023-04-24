﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.GameTicking;
using NUnit.Framework;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.GameRules;

[TestFixture]
public sealed class StartEndGameRulesTest
{
    /// <summary>
    ///     Tests that all game rules can be added/started/ended at the same time without exceptions.
    /// </summary>
    [Test]
    public async Task TestAllConcurrent()
    {
        await using var pairTracker = await PoolManager.GetServerClient(new PoolSettings()
        {
            NoClient = true,
            Dirty = true,
        });
        var server = pairTracker.Pair.Server;
        await server.WaitIdleAsync();
        var gameTicker = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<GameTicker>();

        await server.WaitAssertion(() =>
        {
            var rules = gameTicker.GetAllGameRulePrototypes().ToList();
            rules.Sort((x, y) => string.Compare(x.ID, y.ID, StringComparison.Ordinal));

            // Start all rules
            foreach (var rule in rules)
            {
                gameTicker.StartGameRule(rule.ID);
            }
        });

        // Wait three ticks for any random update loops that might happen
        await server.WaitRunTicks(3);

        await server.WaitAssertion(() =>
        {
            // End all rules
            gameTicker.ClearGameRules();
            Assert.That(!gameTicker.GetAddedGameRules().Any());
        });

        await pairTracker.CleanReturnAsync();
    }
}
