using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Validates consumable definitions before simulation starts.
/// </summary>
public sealed class ConsumableDefinitionValidator : IDefinitionValidator
{
    /// <inheritdoc />
    public void Validate(DefinitionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var errors = registry.Definitions
            .OfType<ConsumableDefinition>()
            .SelectMany(ValidateConsumable)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidDefinitionDataException(string.Join(Environment.NewLine, errors));
        }
    }

    private static IEnumerable<string> ValidateConsumable(ConsumableDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.DisplayName))
        {
            yield return $"Consumable '{definition.Id}' must have a display name.";
        }

        if (definition.Tags is null || definition.Tags.Count == 0)
        {
            yield return $"Consumable '{definition.Id}' must have at least one tag.";
        }
        else
        {
            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tag in definition.Tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    yield return $"Consumable '{definition.Id}' cannot have an empty tag.";
                    continue;
                }

                if (!normalized.Add(tag.Trim()))
                {
                    yield return $"Consumable '{definition.Id}' has duplicate tag '{tag}'.";
                }
            }
        }

        if (!IsFiniteNonNegative(definition.EnergyPerUnit))
        {
            yield return $"Consumable '{definition.Id}' must have finite non-negative energy per unit.";
        }

        if (!IsFiniteNonNegative(definition.HydrationPerUnit))
        {
            yield return $"Consumable '{definition.Id}' must have finite non-negative hydration per unit.";
        }

        if (!IsFiniteNonNegative(definition.ToxicityPerUnit))
        {
            yield return $"Consumable '{definition.Id}' must have finite non-negative toxicity per unit.";
        }

        if (!double.IsFinite(definition.DefaultDigestibility) ||
            definition.DefaultDigestibility < 0 ||
            definition.DefaultDigestibility > 1)
        {
            yield return $"Consumable '{definition.Id}' must have default digestibility between zero and one.";
        }
    }

    private static bool IsFiniteNonNegative(double value)
    {
        return double.IsFinite(value) && value >= 0;
    }
}
