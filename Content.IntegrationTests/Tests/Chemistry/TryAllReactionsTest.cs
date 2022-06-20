using System;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using NUnit.Framework;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests.Chemistry
{
    [TestFixture]
    [TestOf(typeof(ReactionPrototype))]
    public sealed class TryAllReactionsTest
    {
        private const string Prototypes = @"
- type: entity
  id: TestSolutionContainer
  components:
  - type: SolutionContainerManager
    solutions:
      beaker:
        maxVol: 50";
        [Test]
        public async Task TryAllTest()
        {
            await using var pairTracker = await PoolManager.GetServerClient(new PoolSettings{NoClient = true, ExtraPrototypes = Prototypes});
            var server = pairTracker.Pair.Server;

            var entityManager = server.ResolveDependency<IEntityManager>();
            var prototypeManager = server.ResolveDependency<IPrototypeManager>();
            var mapManager = server.ResolveDependency<IMapManager>();
            var coordinates = PoolManager.GetMainEntityCoordinates(mapManager);

            foreach (var reactionPrototype in prototypeManager.EnumeratePrototypes<ReactionPrototype>())
            {
                //since i have no clue how to isolate each loop assert-wise im just gonna throw this one in for good measure
                Console.WriteLine($"Testing {reactionPrototype.ID}");

                EntityUid beaker;
                Solution component = null;

                await server.WaitAssertion(() =>
                {
                    beaker = entityManager.SpawnEntity("TestSolutionContainer", coordinates);
                    Assert.That(EntitySystem.Get<SolutionContainerSystem>()
                        .TryGetSolution(beaker, "beaker", out component));
                    foreach (var (id, reactant) in reactionPrototype.Reactants)
                    {
                        Assert.That(EntitySystem.Get<SolutionContainerSystem>()
                            .TryAddReagent(beaker, component, id, reactant.Amount, out var quantity));
                        Assert.That(reactant.Amount, Is.EqualTo(quantity));
                    }

                    EntitySystem.Get<SolutionContainerSystem>().SetTemperature(beaker, component, reactionPrototype.MinimumTemperature);
                });

                await server.WaitIdleAsync();

                await server.WaitAssertion(() =>
                {
                    //you just got linq'd fool
                    //(i'm sorry)
                    var foundProductsMap = reactionPrototype.Products
                        .Concat(reactionPrototype.Reactants.Where(x => x.Value.Catalyst).ToDictionary(x => x.Key, x => x.Value.Amount))
                        .ToDictionary(x => x, _ => false);
                    foreach (var reagent in component.Contents)
                    {
                        Assert.That(foundProductsMap.TryFirstOrNull(x => x.Key.Key == reagent.ReagentId && x.Key.Value == reagent.Quantity, out var foundProduct));
                        foundProductsMap[foundProduct.Value.Key] = true;
                    }

                    Assert.That(foundProductsMap.All(x => x.Value));
                });

            }
            await pairTracker.CleanReturnAsync();
        }
    }

}
