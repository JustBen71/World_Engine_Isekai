namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Describes the result category of a drinking action.
/// </summary>
public enum DrinkOutcome
{
    /// <summary>
    /// The requested volume was consumed.
    /// </summary>
    Success,

    /// <summary>
    /// Only part of the requested volume was available.
    /// </summary>
    PartialSuccess,

    /// <summary>
    /// The water source exists but is empty.
    /// </summary>
    ResourceEmpty,

    /// <summary>
    /// The source is not accessible from the consumer position.
    /// </summary>
    OutOfRange,

    /// <summary>
    /// The requested volume is not positive and finite.
    /// </summary>
    InvalidQuantity,

    /// <summary>
    /// The referenced consumer entity does not exist.
    /// </summary>
    MissingConsumer,

    /// <summary>
    /// The referenced water source does not exist.
    /// </summary>
    MissingResource,

    /// <summary>
    /// The consumer has no hydration need to receive water.
    /// </summary>
    MissingHydrationNeed
}
