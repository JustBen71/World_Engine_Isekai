namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Stores runtime state for one body part.
/// </summary>
public sealed record BodyPartState(string PartId, double Integrity, double MaxIntegrity);
