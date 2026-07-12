using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Impact;

/// <summary>
/// Describes how an entity resists physical impacts.
/// Values use 1.0 as the standard baseline; higher values resist better.
/// </summary>
public sealed record ImpactResistanceComponent(
    double Hardness,
    double Toughness,
    double FractureResistance) : IComponent;
