using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Tests.TestDoubles;

/// <summary>
/// Data definition used by unit tests.
/// </summary>
public sealed record TestDefinition(DefinitionId Id, string Label) : IDefinition;
