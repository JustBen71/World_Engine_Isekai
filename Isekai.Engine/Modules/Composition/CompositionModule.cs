using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Provides composite entity integration points.
/// </summary>
public sealed class CompositionModule : IEngineModule
{
    /// <inheritdoc />
    public string Name => "Composition";

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
        world.RegisterSystem(new CompositeMembershipSystem());
        world.RegisterSystem(new CompositeIntegritySystem());
        world.RegisterSystem(new CompositeMassSystem());
    }
}
