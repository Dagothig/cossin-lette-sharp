using System.Text.Json.Serialization;
using System;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
    public class EnumConverter<T> : JsonConverter<T> where T : struct, IConvertible
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new Exception();
            var str = reader.GetString()?.ToPascal() ?? string.Empty;
            if (!Enum<T>.Map.TryGetValue(str, out var r))
                throw new Exception($"Unknown value { str } for enum { typeof(T).ToString() }");
            return r;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(EnumConverter<>).MakeGenericType(typeToConvert));
    }
}
