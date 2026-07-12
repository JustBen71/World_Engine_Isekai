using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Modules.Body;

/// <summary>
/// Stores aggregate body integrity from 0 to 1.
/// </summary>
public sealed record BodyIntegrityComponent(double NormalizedIntegrity) : IComponent;
