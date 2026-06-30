using System.Text.Json;
using System.Text.Json.Serialization;
using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Data.Definitions;

/// <summary>
/// Converts definition identifiers between JSON strings and DefinitionId values.
/// </summary>
public sealed class DefinitionIdJsonConverter : JsonConverter<DefinitionId>
{
    /// <inheritdoc />
    public override DefinitionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Definition id must be a string.");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("Definition id cannot be empty.");
        }

        return DefinitionId.From(value);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DefinitionId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
