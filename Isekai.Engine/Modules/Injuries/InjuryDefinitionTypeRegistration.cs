using Isekai.Engine.Data.Definitions;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Registers injury definition types with the generic data loader.
/// </summary>
public static class InjuryDefinitionTypeRegistration
{
    /// <summary>
    /// Registers all injury module definition types.
    /// </summary>
    public static void RegisterInjuryDefinitions(this DefinitionTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.Register<InjuryDefinition>("injury.definition");
    }
}
