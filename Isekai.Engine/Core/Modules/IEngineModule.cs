using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Core.Modules;

/// <summary>
/// Defines the integration points exposed by an engine module.
/// </summary>
public interface IEngineModule
{
    /// <summary>
    /// Gets the module name used for diagnostics and documentation.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Registers JSON definition types owned by this module.
    /// </summary>
    void RegisterDefinitionTypes(DefinitionTypeRegistry registry);

    /// <summary>
    /// Creates validators that must run before the definition registry is frozen.
    /// </summary>
    IReadOnlyCollection<IDefinitionValidator> CreateDefinitionValidators();

    /// <summary>
    /// Registers systems owned by this module into a world.
    /// </summary>
    void RegisterSystems(WorldState world);
}
