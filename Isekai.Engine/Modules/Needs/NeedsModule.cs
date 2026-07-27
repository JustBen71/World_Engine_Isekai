using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Provides needs, consumables, diet and water integration points.
/// </summary>
public sealed class NeedsModule : IEngineModule
{
    private readonly TerrainWaterLayer? _terrainWaterLayer;

    /// <summary>
    /// Creates a needs module without terrain water.
    /// </summary>
    public NeedsModule()
    {
    }

    /// <summary>
    /// Creates a needs module with an optional terrain water layer.
    /// </summary>
    public NeedsModule(TerrainWaterLayer? terrainWaterLayer)
    {
        _terrainWaterLayer = terrainWaterLayer;
    }

    /// <inheritdoc />
    public string Name => "Needs";

    /// <inheritdoc />
    public void RegisterDefinitionTypes(DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.RegisterNeedsDefinitions();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators()
    {
        return new IDefinitionValidator[] { new ConsumableDefinitionValidator() };
    }

    /// <inheritdoc />
    public void RegisterSystems(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        world.RegisterSystem(new EnergyNeedSystem());
        world.RegisterSystem(new HydrationNeedSystem());
        world.RegisterSystem(new ConsumableRegenerationSystem());
        world.RegisterSystem(new WaterSourceRefillSystem());
        world.RegisterSystem(new ConsumeActionSystem());
        world.RegisterSystem(new DrinkActionSystem(_terrainWaterLayer));
    }
}
