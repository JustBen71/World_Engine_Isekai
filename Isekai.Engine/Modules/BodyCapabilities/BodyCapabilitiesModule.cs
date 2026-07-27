using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.BodyCapabilities;

/// <summary>
/// Provides systems for using authorized body parts as physical capabilities.
/// </summary>
public sealed class BodyCapabilitiesModule : IEngineModule
{
    /// <inheritdoc />
    public string Name => "BodyCapabilities";

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
        world.RegisterSystem(new BodyImpactSystem());
    }
}
