using Content.Shared.Item;
using Content.Shared.Stacks;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class StackTest
{
    [Test]
    public async Task StackCorrectItemSize()
    {
        await using var pairTracker = await PoolManager.GetServerClient(new PoolSettings { NoClient = true });
        var server = pairTracker.Pair.Server;

        var protoManager = server.ResolveDependency<IPrototypeManager>();
        var compFact = server.ResolveDependency<IComponentFactory>();

        Assert.Multiple(() =>
        {
            foreach (var entity in PoolManager.GetEntityPrototypes<StackComponent>(server))
            {
                if (!entity.TryGetComponent<StackComponent>(out var stackComponent, compFact) ||
                    !entity.TryGetComponent<ItemComponent>(out var itemComponent, compFact))
                    continue;

                if (!protoManager.TryIndex<StackPrototype>(stackComponent.StackTypeId, out var stackProto) ||
                    stackProto.ItemSize == null)
                    continue;

                var expectedSize = Math.Max(1, (int) MathF.Round(stackProto.ItemSize.Value * stackComponent.Count * stackComponent.SizeMultiplier)); // WD EDIT
                Assert.That(itemComponent.Size, Is.EqualTo(expectedSize), $"Prototype id: {entity.ID} has an item size of {itemComponent.Size} but expected size of {expectedSize}.");
            }
        });

        await pairTracker.CleanReturnAsync();
    }
}
