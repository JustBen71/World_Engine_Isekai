using System.Text.Json;

namespace Isekai.Engine.Data.Definitions;

/// <summary>
/// Provides JSON serializer options used by definition loading.
/// </summary>
public static class JsonDefinitionSerializerOptions
{
    /// <summary>
    /// Creates serializer options for definition JSON.
    /// </summary>
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        options.Converters.Add(new DefinitionIdJsonConverter());
        options.Converters.Add(new DefinitionReferenceJsonConverterFactory());
        return options;
    }
}
