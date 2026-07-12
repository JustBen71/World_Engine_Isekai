namespace Isekai.Engine.Core.Definitions;

/// <summary>
/// Validates a set of registered definitions before simulation starts.
/// </summary>
public interface IDefinitionValidator
{
    /// <summary>
    /// Validates definitions stored in the provided registry.
    /// </summary>
    void Validate(DefinitionRegistry registry);
}
