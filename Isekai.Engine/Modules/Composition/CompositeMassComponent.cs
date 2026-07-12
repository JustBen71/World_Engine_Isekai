using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Composition;

/// <summary>
/// Stores the calculated total mass of a composite entity and its referenced parts.
/// </summary>
public sealed record CompositeMassComponent(double Kilograms) : IComponent;
