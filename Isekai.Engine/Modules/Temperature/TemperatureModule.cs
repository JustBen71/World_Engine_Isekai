using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Provides temperature module integration points.
/// </summary>
public sealed class TemperatureModule : IEngineModule
{
    private readonly IAmbientTemperatureProvider _ambientTemperatureProvider;

    /// <summary>
    /// Creates a temperature module using the default component-based ambient provider.
    /// </summary>
    public TemperatureModule()
        : this(new ComponentAmbientTemperatureProvider())
    {
    }

    /// <summary>
    /// Creates a temperature module using an explicit ambient provider.
    /// </summary>
    public TemperatureModule(IAmbientTemperatureProvider ambientTemperatureProvider)
    {
        _ambientTemperatureProvider = ambientTemperatureProvider ??
                                      throw new ArgumentNullException(nameof(ambientTemperatureProvider));
    }

    /// <inheritdoc />
    public string Name => "Temperature";

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
        world.RegisterSystem(new TemperatureExchangeSystem(_ambientTemperatureProvider));
        world.RegisterSystem(new ThermalComfortSystem());
    }
}
