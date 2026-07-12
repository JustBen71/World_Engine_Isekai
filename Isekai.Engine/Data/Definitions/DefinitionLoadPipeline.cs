using Isekai.Engine.Core.Definitions;
using Isekai.Engine.Diagnostics;

namespace Isekai.Engine.Data.Definitions;

/// <summary>
/// Loads, validates and freezes definition registries for simulation use.
/// </summary>
public sealed class DefinitionLoadPipeline
{
    private readonly JsonDefinitionLoader _loader;
    private readonly IReadOnlyCollection<IDefinitionValidator> _validators;
    private readonly ITraceLogger _traceLogger;

    /// <summary>
    /// Creates a definition loading pipeline.
    /// </summary>
    public DefinitionLoadPipeline(
        JsonDefinitionLoader loader,
        IEnumerable<IDefinitionValidator>? validators = null)
        : this(loader, validators, NoOpTraceLogger.Instance)
    {
    }

    /// <summary>
    /// Creates a definition loading pipeline with a diagnostic logger.
    /// </summary>
    public DefinitionLoadPipeline(
        JsonDefinitionLoader loader,
        IEnumerable<IDefinitionValidator>? validators,
        ITraceLogger traceLogger)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _validators = validators?.ToArray() ?? Array.Empty<IDefinitionValidator>();
        _traceLogger = traceLogger ?? NoOpTraceLogger.Instance;

        using var trace = _traceLogger.BeginScope("DefinitionLoadPipeline.ctor");
    }

    /// <summary>
    /// Loads JSON text, validates all definitions and returns a frozen registry.
    /// </summary>
    public DefinitionRegistry LoadJson(string json, string sourceName = "<memory>")
    {
        using var trace = _traceLogger.BeginScope("DefinitionLoadPipeline.LoadJson");
        var registry = _loader.LoadJson(json, sourceName);
        FinalizeRegistry(registry);
        return registry;
    }

    /// <summary>
    /// Loads a JSON file, validates all definitions and returns a frozen registry.
    /// </summary>
    public DefinitionRegistry LoadFile(string filePath)
    {
        using var trace = _traceLogger.BeginScope("DefinitionLoadPipeline.LoadFile");
        var registry = _loader.LoadFile(filePath);
        FinalizeRegistry(registry);
        return registry;
    }

    /// <summary>
    /// Loads a JSON directory, validates all definitions and returns a frozen registry.
    /// </summary>
    public DefinitionRegistry LoadDirectory(string directoryPath)
    {
        using var trace = _traceLogger.BeginScope("DefinitionLoadPipeline.LoadDirectory");
        var registry = _loader.LoadDirectory(directoryPath);
        FinalizeRegistry(registry);
        return registry;
    }

    private void FinalizeRegistry(DefinitionRegistry registry)
    {
        using var trace = _traceLogger.BeginScope("DefinitionLoadPipeline.FinalizeRegistry");

        registry.EnsureReferencesValid();

        foreach (var validator in _validators)
        {
            using var validatorTrace = _traceLogger.BeginScope($"DefinitionValidator.{validator.GetType().Name}");
            validator.Validate(registry);
        }

        registry.Freeze();
    }
}
