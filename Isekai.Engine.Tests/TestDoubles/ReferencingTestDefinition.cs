using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Tests.TestDoubles;

/// <summary>
/// Data definition used to verify typed references between definitions.
/// </summary>
public sealed record ReferencingTestDefinition(
    DefinitionId Id,
    DefinitionReference<TestDefinition> Primary,
    DefinitionReference<TestDefinition>[] Related) : IDefinition;
