using System.Text.Json.Serialization;
using System;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
    public class FlagsConverter<T> : JsonConverter<Flags<T>> where T : struct, IConvertible
    {
        public override Flags<T> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            var flags = Flags<T>.New();
            while (reader.Read() && reader.TokenType == JsonTokenType.String)
            {
                var value = (T?)JsonSerializer.Deserialize(ref reader, typeof(T), options);
                if (!value.HasValue)
                    throw new Exception();
                else
                    flags[value.Value] = true;
            }
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return flags;
        }

        public override void Write(Utf8JsonWriter writer, Flags<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var (name, flag) in value.Entries)
                writer.WriteStringValue(name);
            writer.WriteEndArray();

        }
    }

    public class FlagsConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(Flags<>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(FlagsConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()));
    }
}
