using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Handling;

/// <summary>
/// Describes an entity's ability to hold and physically manipulate held entities.
/// </summary>
public sealed record GripCapabilityComponent(
    double MaxGripForce,
    double ManipulationForce,
    double Precision) : IComponent;
