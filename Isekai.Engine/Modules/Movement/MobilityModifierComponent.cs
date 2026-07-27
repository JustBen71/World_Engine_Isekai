using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Movement;

/// <summary>
/// Stores a generic mobility multiplier that can represent injury, load or other constraints.
/// </summary>
public sealed record MobilityModifierComponent(double MobilityMultiplier) : IComponent;
