namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Describes the result category of a movement resolution.
/// </summary>
public enum MovementOutcome
{
    /// <summary>The requested distance was fully travelled.</summary>
    Success,
    /// <summary>Only part of the requested distance was travelled.</summary>
    PartialSuccess,
    /// <summary>The intent values are invalid.</summary>
    InvalidIntent,
    /// <summary>The referenced entity is missing.</summary>
    MissingEntity,
    /// <summary>The entity has no position.</summary>
    MissingPosition,
    /// <summary>The entity has no movement capability.</summary>
    MissingCapability,
    /// <summary>The destination is outside the terrain grid.</summary>
    OutOfBounds,
    /// <summary>The destination terrain cannot be traversed.</summary>
    ImpassableTerrain,
    /// <summary>The slope exceeds the entity capacity.</summary>
    SlopeTooSteep,
    /// <summary>The entity has no usable energy.</summary>
    InsufficientEnergy,
    /// <summary>The mobility multiplier is zero.</summary>
    NoMobility
}
