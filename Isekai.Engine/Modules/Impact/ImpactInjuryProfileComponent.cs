using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Configures how impacts received by an entity are converted into injuries.
/// </summary>
public sealed record ImpactInjuryProfileComponent : IComponent
{
    /// <summary>
    /// Creates an impact injury profile.
    /// </summary>
    public ImpactInjuryProfileComponent(IReadOnlyCollection<ImpactInjuryRule> rules)
    {
        Rules = rules?.ToArray() ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    /// Gets the conversion rules for received impacts.
    /// </summary>
    public IReadOnlyCollection<ImpactInjuryRule> Rules { get; }
}
