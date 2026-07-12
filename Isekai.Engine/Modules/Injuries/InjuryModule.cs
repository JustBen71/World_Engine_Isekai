using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Provides injury module integration points.
/// </summary>
public sealed class InjuryModule : IEngineModule
{
    /// <inheritdoc />
    public string Name => "Injuries";

    /// <inheritdoc />
    public void RegisterDefinitionTypes(DefinitionTypeRegistry registry)
    {
        registry.RegisterInjuryDefinitions();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators()
    {
        return new IDefinitionValidator[] { new InjuryDefinitionValidator() };
    }

    /// <inheritdoc />
    public void RegisterSystems(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        world.RegisterSystem(new InjuryApplicationSystem());
        world.RegisterSystem(new BleedingSystem());
    }
}
