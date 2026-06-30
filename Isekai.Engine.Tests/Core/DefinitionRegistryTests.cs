using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Exceptions;
using Isekai.Engine.Tests.TestDoubles;
using Xunit;

namespace Isekai.Engine.Tests.Core;

/// <summary>
/// Tests for neutral data definition registration.
/// </summary>
public sealed class DefinitionRegistryTests
{
    [Fact]
    public void DefinitionId_TrimsStableValue()
    {
        var id = DefinitionId.From(" test.definition ");

        Assert.Equal("test.definition", id.Value);
        Assert.Equal("test.definition", id.ToString());
    }

    [Fact]
    public void DefinitionId_ThrowsWhenValueIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => DefinitionId.From(" "));
    }

    [Fact]
    public void Register_AddsDefinitionToRegistry()
    {
        var registry = new DefinitionRegistry();
        var definition = new TestDefinition(DefinitionId.From("test.definition"), "neutral");

        registry.Register(definition);

        Assert.True(registry.Contains(definition.Id));
        Assert.Same(definition, registry.Get<TestDefinition>(definition.Id));
        Assert.Single(registry.Definitions);
    }

    [Fact]
    public void Register_ThrowsWhenDefinitionIdAlreadyExists()
    {
        var registry = new DefinitionRegistry();
        var definitionId = DefinitionId.From("test.definition");

        registry.Register(new TestDefinition(definitionId, "first"));

        Assert.Throws<DuplicateDefinitionException>(() =>
            registry.Register(new TestDefinition(definitionId, "second")));
    }

    [Fact]
    public void Get_ThrowsWhenDefinitionDoesNotExist()
    {
        var registry = new DefinitionRegistry();
        var definitionId = DefinitionId.From("missing.definition");

        Assert.Throws<DefinitionNotFoundException>(() => registry.Get<TestDefinition>(definitionId));
    }

    [Fact]
    public void Get_ThrowsWhenDefinitionTypeDoesNotMatch()
    {
        var registry = new DefinitionRegistry();
        var definition = new TestDefinition(DefinitionId.From("test.definition"), "neutral");

        registry.Register(definition);

        Assert.Throws<DefinitionTypeMismatchException>(() =>
            registry.Get<AlternativeTestDefinition>(definition.Id));
    }

    [Fact]
    public void TryGet_ReturnsFalseWhenDefinitionIsMissingOrWrongType()
    {
        var registry = new DefinitionRegistry();
        var definition = new TestDefinition(DefinitionId.From("test.definition"), "neutral");

        registry.Register(definition);

        var missingResult = registry.TryGet<TestDefinition>(
            DefinitionId.From("missing.definition"),
            out var missingDefinition);
        var wrongTypeResult = registry.TryGet<AlternativeTestDefinition>(
            definition.Id,
            out var wrongTypeDefinition);

        Assert.False(missingResult);
        Assert.Null(missingDefinition);
        Assert.False(wrongTypeResult);
        Assert.Null(wrongTypeDefinition);
    }

    [Fact]
    public void DefinitionReference_CreatesTypedReference()
    {
        var reference = DefinitionReference<TestDefinition>.From("test.definition");

        Assert.Equal(DefinitionId.From("test.definition"), reference.Id);
        Assert.Equal("test.definition", reference.ToString());
    }

    [Fact]
    public void ValidateReferences_ReturnsValidResultWhenReferencesExistWithExpectedType()
    {
        var registry = new DefinitionRegistry();
        var target = new TestDefinition(DefinitionId.From("target.definition"), "target");
        var owner = new ReferencingTestDefinition(
            DefinitionId.From("owner.definition"),
            DefinitionReference<TestDefinition>.From("target.definition"),
            new[] { DefinitionReference<TestDefinition>.From("target.definition") });

        registry.Register(target);
        registry.Register(owner);

        var result = registry.ValidateReferences();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateReferences_ReturnsErrorsForMissingReferences()
    {
        var registry = new DefinitionRegistry();
        var owner = new ReferencingTestDefinition(
            DefinitionId.From("owner.definition"),
            DefinitionReference<TestDefinition>.From("missing.primary"),
            new[] { DefinitionReference<TestDefinition>.From("missing.related") });

        registry.Register(owner);

        var result = registry.ValidateReferences();

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, error => error.ReferencedDefinitionId == DefinitionId.From("missing.primary"));
        Assert.Contains(result.Errors, error => error.ReferencedDefinitionId == DefinitionId.From("missing.related"));
    }

    [Fact]
    public void ValidateReferences_ReturnsErrorForWrongReferencedType()
    {
        var registry = new DefinitionRegistry();
        var wrongTypeTarget = new AlternativeTestDefinition(DefinitionId.From("target.definition"));
        var owner = new ReferencingTestDefinition(
            DefinitionId.From("owner.definition"),
            DefinitionReference<TestDefinition>.From("target.definition"),
            Array.Empty<DefinitionReference<TestDefinition>>());

        registry.Register(wrongTypeTarget);
        registry.Register(owner);

        var result = registry.ValidateReferences();

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(DefinitionId.From("owner.definition"), error.SourceDefinitionId);
        Assert.Equal(DefinitionId.From("target.definition"), error.ReferencedDefinitionId);
        Assert.Equal(typeof(TestDefinition), error.ExpectedDefinitionType);
    }

    [Fact]
    public void EnsureReferencesValid_ThrowsGroupedValidationException()
    {
        var registry = new DefinitionRegistry();
        var owner = new ReferencingTestDefinition(
            DefinitionId.From("owner.definition"),
            DefinitionReference<TestDefinition>.From("missing.primary"),
            new[] { DefinitionReference<TestDefinition>.From("missing.related") });

        registry.Register(owner);

        var exception = Assert.Throws<DefinitionValidationException>(() => registry.EnsureReferencesValid());

        Assert.Equal(2, exception.Errors.Count);
    }

    [Fact]
    public void Freeze_MarksRegistryAsFrozen()
    {
        var registry = new DefinitionRegistry();

        registry.Freeze();

        Assert.True(registry.IsFrozen);
    }

    [Fact]
    public void Freeze_IsIdempotent()
    {
        var registry = new DefinitionRegistry();

        registry.Freeze();
        registry.Freeze();

        Assert.True(registry.IsFrozen);
    }

    [Fact]
    public void Register_ThrowsWhenRegistryIsFrozen()
    {
        var registry = new DefinitionRegistry();

        registry.Freeze();

        Assert.Throws<DefinitionRegistryFrozenException>(() =>
            registry.Register(new TestDefinition(DefinitionId.From("test.definition"), "neutral")));
    }

    [Fact]
    public void FrozenRegistry_AllowsReadOperations()
    {
        var registry = new DefinitionRegistry();
        var definition = new TestDefinition(DefinitionId.From("test.definition"), "neutral");

        registry.Register(definition);
        registry.Freeze();

        Assert.True(registry.Contains(definition.Id));
        Assert.Same(definition, registry.Get<TestDefinition>(definition.Id));
        Assert.True(registry.TryGet<TestDefinition>(definition.Id, out var foundDefinition));
        Assert.Same(definition, foundDefinition);
    }

    [Fact]
    public void FrozenRegistry_AllowsReferenceValidation()
    {
        var registry = new DefinitionRegistry();
        var target = new TestDefinition(DefinitionId.From("target.definition"), "target");
        var owner = new ReferencingTestDefinition(
            DefinitionId.From("owner.definition"),
            DefinitionReference<TestDefinition>.From("target.definition"),
            Array.Empty<DefinitionReference<TestDefinition>>());

        registry.Register(target);
        registry.Register(owner);
        registry.Freeze();

        var result = registry.ValidateReferences();

        Assert.True(result.IsValid);
    }
}
