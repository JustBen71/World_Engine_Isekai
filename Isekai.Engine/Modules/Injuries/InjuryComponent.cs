using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Injuries;

/// <summary>
/// Stores runtime injuries currently affecting an entity.
/// </summary>
public sealed record InjuryComponent : IComponent
{
    /// <summary>
    /// Creates an injury component.
    /// </summary>
    public InjuryComponent(IReadOnlyCollection<InjuryState> injuries)
    {
        Injuries = injuries?.ToArray() ?? throw new ArgumentNullException(nameof(injuries));
    }

    /// <summary>
    /// Gets all injuries currently affecting the entity.
    /// </summary>
    public IReadOnlyCollection<InjuryState> Injuries { get; }
}
