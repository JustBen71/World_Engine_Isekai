using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Temperature;

/// <summary>
/// Stores ambient temperature available to temperature systems.
/// </summary>
public sealed record AmbientTemperatureComponent(double Celsius) : IComponent;
