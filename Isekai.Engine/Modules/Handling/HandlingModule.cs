using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Handling;

/// <summary>
/// Provides generic holding and handled-impact integration points.
/// </summary>
public sealed class HandlingModule : IEngineModule
{
    /// <inheritdoc />
    public string Name => "Handling";

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
        world.RegisterSystem(new HandledImpactSystem());
    }
}
