namespace Isekai.Engine.Sandbox.Data.Entities;

/// <summary>
/// JSON model for an entity's ability to hold and manipulate entities.
/// </summary>
public sealed record SandboxGripCapabilityData(
    double MaxGripForce,
    double ManipulationForce,
    double Precision);
