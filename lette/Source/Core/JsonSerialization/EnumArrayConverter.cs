using System.Text.Json.Serialization;
using System;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
    public class EnumArrayConverter<K, V> : MapConverter<EnumArray<K, V>>
    where K : struct, IConvertible
    where V : new()
    {
        public override EnumArray<K, V> Init() => EnumArray<K, V>.New();

        public override void ReadValue(
            ref Utf8JsonReader reader,
            ref EnumArray<K, V> arr,
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

        public override void Write(Utf8JsonWriter writer, EnumArray<K, V> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumArrayConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(EnumArray<,>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(EnumArrayConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
    }
}
