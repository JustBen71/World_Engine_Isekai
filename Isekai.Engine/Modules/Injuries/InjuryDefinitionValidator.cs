using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Validates injury definitions before simulation starts.
/// </summary>
public sealed class InjuryDefinitionValidator : IDefinitionValidator
{
    /// <inheritdoc />
    public void Validate(DefinitionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var errors = registry.Definitions
            .OfType<InjuryDefinition>()
            .SelectMany(ValidateInjury)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidDefinitionDataException(string.Join(Environment.NewLine, errors));
        }
    }

    private static IEnumerable<string> ValidateInjury(InjuryDefinition injury)
    {
        if (string.IsNullOrWhiteSpace(injury.Name))
        {
            yield return $"Injury '{injury.Id}' must have a name.";
        }

        if (injury.IntegrityLossPerSeverityPerSecond < 0)
        {
            yield return $"Injury '{injury.Id}' cannot have negative integrity loss.";
        }
    }
}
