using Isekai.Engine.Core;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Core.Modules;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Provides body module integration points.
/// </summary>
public sealed class BodyModule : IEngineModule
{
    /// <inheritdoc />
    public string Name => "Body";

    /// <inheritdoc />
    public void RegisterDefinitionTypes(DefinitionTypeRegistry registry)
    {
        registry.RegisterBodyDefinitions();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators()
    {
        return new IDefinitionValidator[] { new BodyDefinitionValidator() };
    }

    /// <inheritdoc />
    public void RegisterSystems(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        world.RegisterSystem(new BodyInitializationSystem());
        world.RegisterSystem(new BodyIntegritySystem());
    }
}
