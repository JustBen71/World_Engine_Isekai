using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;
using Isekai.Engine.Modules.Terrain;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Provides movement module integration points.
/// </summary>
public sealed class MovementModule : IEngineModule
{
    private readonly ITerrainService? _terrainService;
    private readonly IMobilityProvider? _mobilityProvider;
    private readonly ITerrainTraversalProvider? _traversalProvider;

    /// <summary>
    /// Creates a movement module without runtime registration.
    /// </summary>
    public MovementModule()
    {
    }

    /// <summary>
    /// Creates a movement module with terrain access.
    /// </summary>
    public MovementModule(
        ITerrainService terrainService,
        IMobilityProvider? mobilityProvider = null,
        ITerrainTraversalProvider? traversalProvider = null)
    {
        _terrainService = terrainService ?? throw new ArgumentNullException(nameof(terrainService));
        _mobilityProvider = mobilityProvider;
        _traversalProvider = traversalProvider;
    }

    /// <inheritdoc />
    public string Name => "Movement";

    /// <inheritdoc />
    public void RegisterDefinitionTypes(DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators()
    {
        return Array.Empty<IDefinitionValidator>();
    }

    /// <inheritdoc />
    public void RegisterSystems(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (_terrainService is not null)
        {
            world.RegisterSystem(new MovementResolutionSystem(_terrainService, _mobilityProvider, _traversalProvider));
        }
    }
}
