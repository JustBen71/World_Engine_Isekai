using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Modules.BodyCapabilities;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for generic body capability module integration.
/// </summary>
public sealed class BodyCapabilitiesModuleTests
{
    [Fact]
    public void BodyCapabilitiesModule_RegistersBodyImpactSystem()
    {
        var world = CreateWorld();
        var module = new BodyCapabilitiesModule();

        module.RegisterSystems(world);

        Assert.Contains(world.Systems, system => system is BodyImpactSystem);
    }

    private static WorldState CreateWorld()
    {
        var registry = new DefinitionRegistry();
        registry.Freeze();

        return new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);
    }
}
