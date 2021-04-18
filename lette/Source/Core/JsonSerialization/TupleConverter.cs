using System.Text.Json.Serialization;
using System;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
    public class TupleConverter<A, B> : JsonConverter<(A, B)> where A : notnull where B : notnull
    {
        public override (A, B) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            if (!reader.Read() || reader.TokenType == JsonTokenType.EndArray)
                throw new Exception();
            var a = JsonSerializer.Deserialize<A>(ref reader, options);
            if (!reader.Read() || reader.TokenType == JsonTokenType.EndArray)
                throw new Exception();
            var b = JsonSerializer.Deserialize<B>(ref reader, options);
            if (a == null || b == null || !reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return (a, b);
        }

        public override void Write(Utf8JsonWriter writer, (A, B) value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class TupleConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(ValueTuple<,>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(TupleConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
    }
}
