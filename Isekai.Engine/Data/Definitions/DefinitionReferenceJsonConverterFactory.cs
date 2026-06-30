using System.Text.Json;
using System.Text.Json.Serialization;
using Isekai.Engine.Core.Definitions;

namespace Isekai.Engine.Data.Definitions;

/// <summary>
/// Creates JSON converters for typed definition references.
/// </summary>
public sealed class DefinitionReferenceJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(DefinitionReference<>);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var definitionType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(DefinitionReferenceJsonConverter<>).MakeGenericType(definitionType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class DefinitionReferenceJsonConverter<TDefinition> :
        JsonConverter<DefinitionReference<TDefinition>>
        where TDefinition : IDefinition
    {
        public override DefinitionReference<TDefinition> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Definition reference must be a string.");
            }

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException("Definition reference cannot be empty.");
            }

            return DefinitionReference<TDefinition>.From(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DefinitionReference<TDefinition> value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Id.Value);
        }
    }
}
