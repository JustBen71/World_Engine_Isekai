using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Modules.Materials;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for the complete definition loading pipeline.
/// </summary>
public sealed class DefinitionLoadPipelineTests
{
    [Fact]
    public void LoadJson_ReturnsFrozenValidatedRegistry()
    {
        var pipeline = CreateMaterialPipeline();
        const string json = """
        {
          "type": "material.definition",
          "id": "material.water",
          "density": 1000,
          "specificHeatCapacity": 4200,
          "thermalConductivity": 0.6
        }
        """;

        var registry = pipeline.LoadJson(json);

        Assert.True(registry.IsFrozen);
        Assert.True(registry.Contains(DefinitionId.From("material.water")));
    }

    [Fact]
    public void LoadJson_ThrowsWhenReferenceValidationFails()
    {
        var types = new DefinitionTypeRegistry();
        types.Register<ReferencingPipelineDefinition>("referencing.definition");
        var pipeline = new DefinitionLoadPipeline(new JsonDefinitionLoader(types));

        const string json = """
        {
          "type": "referencing.definition",
          "id": "owner.definition",
          "target": "missing.definition"
        }
        """;

        Assert.Throws<DefinitionValidationException>(() => pipeline.LoadJson(json));
    }

    [Fact]
    public void LoadJson_ThrowsWhenModuleValidatorFails()
    {
        var pipeline = CreateMaterialPipeline();
        const string json = """
        {
          "type": "material.definition",
          "id": "material.invalid",
          "density": 0,
          "specificHeatCapacity": 4200,
          "thermalConductivity": 0.6
        }
        """;

        Assert.Throws<InvalidDefinitionDataException>(() => pipeline.LoadJson(json));
    }

    [Fact]
    public void LoadJson_ResultCanBeUsedByWorldState()
    {
        var pipeline = CreateMaterialPipeline();
        const string json = """
        {
          "type": "material.definition",
          "id": "material.water",
          "density": 1000,
          "specificHeatCapacity": 4200,
          "thermalConductivity": 0.6
        }
        """;

        var registry = pipeline.LoadJson(json);
        var world = new WorldState(
            new SimulationTime(),
            new EventBus(),
            registry,
            NoOpTraceLogger.Instance);

        Assert.True(world.Definitions.IsFrozen);
        Assert.True(world.Definitions.Contains(DefinitionId.From("material.water")));
    }

    private static DefinitionLoadPipeline CreateMaterialPipeline()
    {
        var types = new DefinitionTypeRegistry();
        types.RegisterMaterialDefinitions();

        return new DefinitionLoadPipeline(
            new JsonDefinitionLoader(types),
            new[] { new MaterialDefinitionValidator() });
    }

    /// <summary>
    /// Definition used to verify pipeline reference validation.
    /// </summary>
    private sealed record ReferencingPipelineDefinition(
        DefinitionId Id,
        DefinitionReference<MaterialDefinition> Target) : IDefinition;
}
