using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Tests.TestDoubles;

/// <summary>
/// Second data-only component used to verify component collections.
/// </summary>
public sealed record SecondTestComponent(string Name) : IComponent;
