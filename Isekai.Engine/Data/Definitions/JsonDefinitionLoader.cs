using System.Text.Json;
using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;
using Isekai.Engine.Exceptions;

namespace Isekai.Engine.Data.Definitions;

/// <summary>
/// Loads data-driven definitions from JSON documents.
/// </summary>
public sealed class JsonDefinitionLoader
{
    private readonly DefinitionTypeRegistry _definitionTypes;
    private readonly ITraceLogger _traceLogger;

    /// <summary>
    /// Creates a JSON definition loader.
    /// </summary>
    public JsonDefinitionLoader(DefinitionTypeRegistry definitionTypes)
        : this(definitionTypes, NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates a JSON definition loader with a diagnostic logger.
    /// </summary>
    public JsonDefinitionLoader(DefinitionTypeRegistry definitionTypes, ITraceLogger traceLogger)
    {
        _definitionTypes = definitionTypes ?? throw new ArgumentNullException(nameof(definitionTypes));
        _traceLogger = traceLogger ?? NoOpTraceLogger.Instance;

        using var trace = _traceLogger.BeginScope("JsonDefinitionLoader.ctor");
    }

    /// <summary>
    /// Loads every JSON definition file in a directory into a new registry.
    /// Files are processed recursively in ordinal path order.
    /// </summary>
    public DefinitionRegistry LoadDirectory(string directoryPath)
    {
        using var trace = _traceLogger.BeginScope("JsonDefinitionLoader.LoadDirectory");
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            throw new DefinitionDataDirectoryNotFoundException(directoryPath);
        }

        var registry = new DefinitionRegistry(_traceLogger);
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            LoadFileInto(filePath, registry);
        }

        return registry;
    }

    /// <summary>
    /// Loads one JSON file into a new registry.
    /// </summary>
    public DefinitionRegistry LoadFile(string filePath)
    {
        using var trace = _traceLogger.BeginScope("JsonDefinitionLoader.LoadFile");

        var registry = new DefinitionRegistry(_traceLogger);
        LoadFileInto(filePath, registry);
        return registry;
    }

    /// <summary>
    /// Loads one JSON file into an existing registry.
    /// </summary>
    public void LoadFileInto(string filePath, DefinitionRegistry registry)
    {
        using var trace = _traceLogger.BeginScope("JsonDefinitionLoader.LoadFileInto");
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(registry);

        if (!File.Exists(filePath))
        {
            throw new DefinitionDataFileNotFoundException(filePath);
        }

        var json = File.ReadAllText(filePath);
        LoadJsonInto(json, registry, filePath);
    }

    /// <summary>
    /// Loads JSON text into a new registry.
    /// </summary>
    public DefinitionRegistry LoadJson(string json, string sourceName = "<memory>")
    {
        using var trace = _traceLogger.BeginScope("JsonDefinitionLoader.LoadJson");

        var registry = new DefinitionRegistry(_traceLogger);
        LoadJsonInto(json, registry, sourceName);
        return registry;
    }

    /// <summary>
    /// Loads JSON text into an existing registry.
    /// </summary>
    public void LoadJsonInto(string json, DefinitionRegistry registry, string sourceName = "<memory>")
    {
        using var trace = _traceLogger.BeginScope("JsonDefinitionLoader.LoadJsonInto");
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentNullException.ThrowIfNull(registry);

        using var document = ParseJson(json, sourceName);
        var definitions = GetDefinitionElements(document.RootElement, sourceName);

        foreach (var definitionElement in definitions)
        {
            var id = ReadRequiredString(definitionElement, "id", sourceName);
            var type = ReadRequiredString(definitionElement, "type", sourceName);

            _traceLogger.Write($"Definition: {id} ({type})");

            var definition = _definitionTypes.Create(type, definitionElement);
            if (definition.Id != DefinitionId.From(id))
            {
                throw new InvalidDefinitionDataException(
                    $"Definition '{id}' deserialized with mismatched id '{definition.Id}' in '{sourceName}'.");
            }

            registry.Register(definition);
        }
    }

    private static JsonDocument ParseJson(string json, string sourceName)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException exception)
        {
            throw new InvalidDefinitionJsonException(sourceName, exception);
        }
    }

    private static IReadOnlyCollection<JsonElement> GetDefinitionElements(JsonElement root, string sourceName)
    {
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("definitions", out var definitionsElement))
        {
            if (definitionsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDefinitionDataException(
                    $"Property 'definitions' in '{sourceName}' must be an array.");
            }

            return definitionsElement.EnumerateArray().Select(CloneDefinitionElement).ToArray();
        }

        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.EnumerateArray().Select(CloneDefinitionElement).ToArray();
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            return new[] { root.Clone() };
        }

        throw new InvalidDefinitionDataException(
            $"Root JSON value in '{sourceName}' must be an object or an array.");
    }

    private static JsonElement CloneDefinitionElement(JsonElement definitionElement)
    {
        if (definitionElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDefinitionDataException("Each definition entry must be a JSON object.");
        }

        return definitionElement.Clone();
    }

    private static string ReadRequiredString(JsonElement definitionElement, string propertyName, string sourceName)
    {
        if (!definitionElement.TryGetProperty(propertyName, out var property))
        {
            throw new MissingDefinitionFieldException(propertyName, sourceName);
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidDefinitionDataException(
                $"Property '{propertyName}' in '{sourceName}' must be a string.");
        }

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDefinitionDataException(
                $"Property '{propertyName}' in '{sourceName}' cannot be empty.");
        }

        return value;
    }
}
