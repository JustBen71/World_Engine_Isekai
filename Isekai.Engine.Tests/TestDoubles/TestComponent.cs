using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Tests.TestDoubles;

/// <summary>
/// Data-only component used by unit tests.
/// </summary>
public sealed record TestComponent(int Value) : IComponent;
