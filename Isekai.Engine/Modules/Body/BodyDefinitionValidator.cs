using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Modules.Materials;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Validates body definitions before simulation starts.
/// </summary>
public sealed class BodyDefinitionValidator : IDefinitionValidator
{
    /// <inheritdoc />
    public void Validate(DefinitionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var errors = registry.Definitions
            .OfType<BodyDefinition>()
            .SelectMany(body => ValidateBody(registry, body))
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidDefinitionDataException(string.Join(Environment.NewLine, errors));
        }
    }

    private static IEnumerable<string> ValidateBody(DefinitionRegistry registry, BodyDefinition body)
    {
        if (body.Parts.Length == 0)
        {
            yield return $"Body '{body.Id}' must define at least one part.";
        }

        var partIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var part in body.Parts)
        {
            if (string.IsNullOrWhiteSpace(part.Id))
            {
                yield return $"Body '{body.Id}' contains a part with an empty id.";
            }
            else if (!partIds.Add(part.Id))
            {
                yield return $"Body '{body.Id}' contains duplicate part id '{part.Id}'.";
            }

            if (string.IsNullOrWhiteSpace(part.Name))
            {
                yield return $"Body '{body.Id}' part '{part.Id}' must have a name.";
            }

            if (part.MaxIntegrity <= 0)
            {
                yield return $"Body '{body.Id}' part '{part.Id}' must have max integrity greater than zero.";
            }

            if (part.VitalRole is not null && string.IsNullOrWhiteSpace(part.VitalRole))
            {
                yield return $"Body '{body.Id}' part '{part.Id}' has an empty vital role.";
            }

            if (!registry.TryGet<MaterialDefinition>(part.Material.Id, out _))
            {
                yield return $"Body '{body.Id}' part '{part.Id}' references missing material '{part.Material.Id}'.";
            }
        }
    }
}
