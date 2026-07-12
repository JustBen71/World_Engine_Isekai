using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Provides generic contact and impact integration points.
/// </summary>
public sealed class ImpactModule : IEngineModule
{
    /// <inheritdoc />
    public string Name => "Impact";

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
        world.RegisterSystem(new ImpactResolutionSystem());
        world.RegisterSystem(new ImpactToBodyDamageSystem());
        world.RegisterSystem(new ImpactContactWearSystem());
    }
}
