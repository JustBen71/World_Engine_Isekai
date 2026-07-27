namespace Isekai.Engine.Modules.Needs;

/// <summary>
/// Describes the result category of a food consumption action.
/// </summary>
public enum ConsumeOutcome
{
    /// <summary>
    /// The requested quantity was consumed.
    /// </summary>
    Success,

    /// <summary>
    /// Only part of the requested quantity was available.
    /// </summary>
    PartialSuccess,

    /// <summary>
    /// The resource exists but has no available quantity.
    /// </summary>
    ResourceEmpty,

    /// <summary>
    /// The consumer diet cannot digest this resource.
    /// </summary>
    NotDigestible,

    /// <summary>
    /// The resource is not accessible from the consumer position.
    /// </summary>
    OutOfRange,

    /// <summary>
    /// The requested quantity is not positive and finite.
    /// </summary>
    InvalidQuantity,

    /// <summary>
    /// The referenced consumer entity does not exist.
    /// </summary>
    MissingConsumer,

    /// <summary>
    /// The referenced resource entity does not exist or is not consumable.
    /// </summary>
    MissingResource,

    /// <summary>
    /// The consumer has no diet component.
    /// </summary>
    MissingDiet,

    /// <summary>
    /// The consumer has no energy or hydration need to receive nutrition.
    /// </summary>
    MissingNeeds
}
