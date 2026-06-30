using System.Collections;
using System.Reflection;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Core.Definitions;

/// <summary>
/// Stores data definitions by stable identifier.
/// </summary>
public sealed class DefinitionRegistry
{
    private readonly Dictionary<DefinitionId, IDefinition> _definitions = new();
    private readonly ITraceLogger _traceLogger;
    private bool _isFrozen;

    /// <summary>
    /// Creates an empty definition registry without diagnostic output.
    /// </summary>
    public DefinitionRegistry()
        : this(NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates an empty definition registry with a diagnostic logger.
    /// </summary>
    public DefinitionRegistry(ITraceLogger traceLogger)
    {
        _traceLogger = traceLogger ?? NoOpTraceLogger.Instance;

        using var trace = _traceLogger.BeginScope("DefinitionRegistry.ctor");
    }

    /// <summary>
    /// Gets all registered definitions.
    /// </summary>
    public IReadOnlyCollection<IDefinition> Definitions => _definitions.Values.ToArray();

    /// <summary>
    /// Returns true when the registry no longer accepts new definitions.
    /// </summary>
    public bool IsFrozen => _isFrozen;

    /// <summary>
    /// Registers a definition.
    /// </summary>
    public void Register<TDefinition>(TDefinition definition)
        where TDefinition : IDefinition
    {
        using var trace = _traceLogger.BeginScope("DefinitionRegistry.Register");
        ArgumentNullException.ThrowIfNull(definition);

        if (_isFrozen)
        {
            throw new DefinitionRegistryFrozenException();
        }

        _traceLogger.Write($"DefinitionId: {definition.Id}");

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new DuplicateDefinitionException(definition.Id);
        }

        _definitions.Add(definition.Id, definition);
    }

    /// <summary>
    /// Returns true when a definition with the requested identifier exists.
    /// </summary>
    public bool Contains(DefinitionId definitionId)
    {
        using var trace = _traceLogger.BeginScope("DefinitionRegistry.Contains");
        return _definitions.ContainsKey(definitionId);
    }

    /// <summary>
    /// Gets a definition by identifier and expected type.
    /// </summary>
    public TDefinition Get<TDefinition>(DefinitionId definitionId)
        where TDefinition : IDefinition
    {
        using var trace = _traceLogger.BeginScope("DefinitionRegistry.Get");

        if (!_definitions.TryGetValue(definitionId, out var definition))
        {
            throw new DefinitionNotFoundException(definitionId);
        }

        if (definition is not TDefinition typedDefinition)
        {
            throw new DefinitionTypeMismatchException(definitionId, typeof(TDefinition), definition.GetType());
        }

        return typedDefinition;
    }

    /// <summary>
    /// Tries to get a definition by identifier and expected type.
    /// </summary>
    public bool TryGet<TDefinition>(DefinitionId definitionId, out TDefinition? definition)
        where TDefinition : IDefinition
    {
        using var trace = _traceLogger.BeginScope("DefinitionRegistry.TryGet");

        if (_definitions.TryGetValue(definitionId, out var storedDefinition) &&
            storedDefinition is TDefinition typedDefinition)
        {
            definition = typedDefinition;
            return true;
        }

        definition = default;
        return false;
    }

    /// <summary>
    /// Freezes the registry so definitions cannot be added after loading and validation.
    /// Calling this method more than once is safe.
    /// </summary>
    public void Freeze()
    {
        using var trace = _traceLogger.BeginScope("DefinitionRegistry.Freeze");
        _isFrozen = true;
    }

    /// <summary>
    /// Validates typed references declared by registered definitions.
    /// </summary>
    public DefinitionValidationResult ValidateReferences()
    {
        using var trace = _traceLogger.BeginScope("DefinitionRegistry.ValidateReferences");

        var errors = new List<DefinitionValidationError>();
        foreach (var definition in _definitions.Values)
        {
            foreach (var reference in EnumerateReferences(definition))
            {
                if (!_definitions.TryGetValue(reference.ReferencedDefinitionId, out var referencedDefinition))
                {
                    errors.Add(new DefinitionValidationError(
                        definition.Id,
                        reference.PropertyName,
                        reference.ReferencedDefinitionId,
                        reference.ExpectedDefinitionType,
                        $"Definition '{definition.Id}' references missing definition '{reference.ReferencedDefinitionId}' through '{reference.PropertyName}'."));

                    continue;
                }

                if (!reference.ExpectedDefinitionType.IsInstanceOfType(referencedDefinition))
                {
                    errors.Add(new DefinitionValidationError(
                        definition.Id,
                        reference.PropertyName,
                        reference.ReferencedDefinitionId,
                        reference.ExpectedDefinitionType,
                        $"Definition '{definition.Id}' references '{reference.ReferencedDefinitionId}' through '{reference.PropertyName}', but the registered definition has type '{referencedDefinition.GetType().Name}'."));
                }
            }
        }

        return new DefinitionValidationResult(errors);
    }

    /// <summary>
    /// Throws when typed references declared by registered definitions are invalid.
    /// </summary>
    public void EnsureReferencesValid()
    {
        using var trace = _traceLogger.BeginScope("DefinitionRegistry.EnsureReferencesValid");

        var result = ValidateReferences();
        if (!result.IsValid)
        {
            throw new DefinitionValidationException(result.Errors);
        }
    }

    private static IEnumerable<ReferenceInfo> EnumerateReferences(IDefinition definition)
    {
        var properties = definition.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = property.GetValue(definition);
            if (TryGetReferenceType(property.PropertyType, out var expectedType))
            {
                if (value is not null)
                {
                    yield return new ReferenceInfo(
                        property.Name,
                        GetReferenceId(value),
                        expectedType);
                }

                continue;
            }

            if (TryGetReferenceCollectionType(property.PropertyType, out expectedType) && value is IEnumerable items)
            {
                foreach (var item in items)
                {
                    if (item is not null)
                    {
                        yield return new ReferenceInfo(
                            property.Name,
                            GetReferenceId(item),
                            expectedType);
                    }
                }
            }
        }
    }

    private static bool TryGetReferenceType(Type type, out Type expectedDefinitionType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DefinitionReference<>))
        {
            expectedDefinitionType = type.GetGenericArguments()[0];
            return true;
        }

        expectedDefinitionType = typeof(IDefinition);
        return false;
    }

    private static bool TryGetReferenceCollectionType(Type type, out Type expectedDefinitionType)
    {
        var enumerableTypes = type.IsInterface
            ? type.GetInterfaces().Append(type)
            : type.GetInterfaces();

        foreach (var enumerableType in enumerableTypes)
        {
            if (!enumerableType.IsGenericType ||
                enumerableType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                continue;
            }

            var itemType = enumerableType.GetGenericArguments()[0];
            if (TryGetReferenceType(itemType, out expectedDefinitionType))
            {
                return true;
            }
        }

        expectedDefinitionType = typeof(IDefinition);
        return false;
    }

    private static DefinitionId GetReferenceId(object reference)
    {
        var idProperty = reference.GetType().GetProperty(nameof(DefinitionReference<IDefinition>.Id));
        if (idProperty?.GetValue(reference) is DefinitionId id)
        {
            return id;
        }

        throw new InvalidOperationException("Definition reference does not expose a valid id.");
    }

    private sealed record ReferenceInfo(
        string PropertyName,
        DefinitionId ReferencedDefinitionId,
        Type ExpectedDefinitionType);
}
