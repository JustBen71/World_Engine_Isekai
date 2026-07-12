using Isekai.Engine.Core.Component;
using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Stores runtime state for an entity body.
/// </summary>
public sealed record BodyStateComponent : IComponent
{
    /// <summary>
    /// Creates body runtime state.
    /// </summary>
    public BodyStateComponent(DefinitionReference<BodyDefinition> Body, IReadOnlyCollection<BodyPartState> Parts)
    {
        this.Body = Body;
        this.Parts = Parts?.ToArray() ?? throw new ArgumentNullException(nameof(Parts));
    }

    /// <summary>
    /// Gets the body definition used to initialize the state.
    /// </summary>
    public DefinitionReference<BodyDefinition> Body { get; }

    /// <summary>
    /// Gets runtime state for every body part.
    /// </summary>
    public IReadOnlyCollection<BodyPartState> Parts { get; }
}
