using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Terrain;

/// <summary>
/// Registers terrain definition types with the generic data loader.
/// </summary>
public static class TerrainDefinitionTypeRegistration
{
    /// <summary>
    /// Registers all terrain module definition types.
    /// </summary>
    public static void RegisterTerrainDefinitions(this DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.Register<SoilDefinition>("terrain.soil");
        registry.Register<TerrainGridDefinition>("terrain.grid");
    }
}
