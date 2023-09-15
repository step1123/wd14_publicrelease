using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Toolshed;

namespace Content.IntegrationTests.Tests.Toolshed;

[TestFixture]
public sealed class AdminTest : ToolshedTest
{
    [Test]
    [Ignore("Have errors in engine that can't be fixed right now.")] //WD-edit
    public async Task AllCommandsHavePermissions()
    {
        await Server.WaitAssertion(() =>
        {
            Assert.That(InvokeCommand("cmd:list where { acmd:perms isnull }", out var res));
            var list = ((IEnumerable<CommandSpec>) res).ToList();
            Assert.That(list, Is.Empty, "All commands must have admin permissions set up.");
        });
    }
}
