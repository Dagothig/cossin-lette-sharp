using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
    public class EnumDictionaryConverter<K, V> : MapConverter<Dictionary<K, V>>
    where K : struct, IConvertible
    where V : new()
    {
        public override Dictionary<K, V> Init() => new();

        public override void ReadValue(
            ref Utf8JsonReader reader,
            ref Dictionary<K, V> arr,
            string name,
            JsonSerializerOptions options)
        {
            if (!Enum<K>.Map.TryGetValue(name.ToPascal(), out var key))
                throw new Exception($"Unknown value { name } for enum { typeof(K).ToString() }");
            var value = JsonSerializer.Deserialize<V>(ref reader, options);
            if (value == null)
                throw new Exception();
            arr[key] = value;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<K, V> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumDictionaryConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
            typeToConvert.GetGenericArguments()[0].IsEnum;

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(EnumDictionaryConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
    }
}
