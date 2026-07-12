using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Materials;

/// <summary>
/// Stores the calculated mass of an entity in kilograms.
/// </summary>
public sealed record MaterialMassComponent(double Kilograms) : IComponent;
