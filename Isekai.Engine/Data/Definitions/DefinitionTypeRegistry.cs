using System.Text.Json;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Data.Definitions;

/// <summary>
/// Maps JSON definition type names to concrete definition factories.
/// </summary>
public sealed class DefinitionTypeRegistry
{
    private readonly Dictionary<string, Func<JsonElement, IDefinition>> _factories =
        new(StringComparer.Ordinal);

    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITraceLogger _traceLogger;

    /// <summary>
    /// Creates an empty definition type registry.
    /// </summary>
    public DefinitionTypeRegistry()
        : this(NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates an empty definition type registry with a diagnostic logger.
    /// </summary>
    public DefinitionTypeRegistry(ITraceLogger traceLogger)
    {
        _traceLogger = traceLogger ?? NoOpTraceLogger.Instance;
        _jsonOptions = JsonDefinitionSerializerOptions.Create();

        using var trace = _traceLogger.BeginScope("DefinitionTypeRegistry.ctor");
    }

    /// <summary>
    /// Gets all registered JSON type names.
    /// </summary>
    public IReadOnlyCollection<string> TypeNames => _factories.Keys.ToArray();

    /// <summary>
    /// Registers a concrete definition type for a JSON type name.
    /// </summary>
    public void Register<TDefinition>(string typeName)
        where TDefinition : IDefinition
    {
        using var trace = _traceLogger.BeginScope("DefinitionTypeRegistry.Register");
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        var normalizedTypeName = typeName.Trim();
        _traceLogger.Write($"TypeName: {normalizedTypeName}");

        if (_factories.ContainsKey(normalizedTypeName))
        {
            throw new DuplicateDefinitionTypeException(normalizedTypeName);
        }

        _factories.Add(normalizedTypeName, Deserialize<TDefinition>);
    }

    /// <summary>
    /// Creates a definition from a registered JSON type name and JSON object.
    /// </summary>
    public IDefinition Create(string typeName, JsonElement definitionElement)
    {
        using var trace = _traceLogger.BeginScope("DefinitionTypeRegistry.Create");
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        var normalizedTypeName = typeName.Trim();
        if (!_factories.TryGetValue(normalizedTypeName, out var factory))
        {
            throw new UnknownDefinitionTypeException(normalizedTypeName);
        }

        return factory(definitionElement);
    }

    private IDefinition Deserialize<TDefinition>(JsonElement definitionElement)
        where TDefinition : IDefinition
    {
        TDefinition? definition;
        try
        {
            definition = definitionElement.Deserialize<TDefinition>(_jsonOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidDefinitionDataException(
                $"Definition JSON could not be deserialized as '{typeof(TDefinition).Name}'.",
                exception);
        }

        if (definition is null)
        {
            throw new InvalidDefinitionDataException("Definition deserialization returned null.");
        }

        return definition;
    }
}
