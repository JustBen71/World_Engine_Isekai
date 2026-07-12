namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Describes the generic physical outcome of an impact.
/// </summary>
public enum ImpactOutcome
{
    /// <summary>
    /// The impact did not overcome the target resistance.
    /// </summary>
    None,

    /// <summary>
    /// The impact lightly marked the target.
    /// </summary>
    Scratch,

    /// <summary>
    /// The impact deformed the target without cutting or piercing it.
    /// </summary>
    Dent,

    /// <summary>
    /// The impact cracked the target.
    /// </summary>
    Crack,

    /// <summary>
    /// The impact cut the target.
    /// </summary>
    Cut,

    /// <summary>
    /// The impact punctured the target.
    /// </summary>
    Puncture,

    /// <summary>
    /// The impact strongly fractured the target.
    /// </summary>
    Fracture
}
