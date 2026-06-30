using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Tests.TestDoubles;

/// <summary>
/// Alternative data definition used to verify typed registry access.
/// </summary>
public sealed record AlternativeTestDefinition(DefinitionId Id) : IDefinition;
