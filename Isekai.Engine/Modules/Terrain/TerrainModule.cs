using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Provides terrain module integration points.
/// </summary>
public sealed class TerrainModule : IEngineModule
{
    /// <summary>
    /// Creates a terrain module without a pre-built terrain service.
    /// </summary>
    public TerrainModule()
    {
    }

    /// <summary>
    /// Creates a terrain module with an explicit terrain service.
    /// </summary>
    public TerrainModule(ITerrainService terrainService)
    {
        TerrainService = terrainService ?? throw new ArgumentNullException(nameof(terrainService));
    }

    /// <inheritdoc />
    public string Name => "Terrain";

    /// <summary>
    /// Gets the terrain service associated with this module, when one was provided by the host.
    /// </summary>
    public ITerrainService? TerrainService { get; }

    /// <inheritdoc />
    public void RegisterDefinitionTypes(DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.RegisterTerrainDefinitions();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators()
    {
        return new IDefinitionValidator[]
        {
            new SoilDefinitionValidator(),
            new TerrainGridDefinitionValidator()
        };
    }

    /// <inheritdoc />
    public void RegisterSystems(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
    }
}
