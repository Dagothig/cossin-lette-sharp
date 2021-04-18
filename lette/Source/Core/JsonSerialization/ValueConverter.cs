using System.Text.Json.Serialization;
using System;
using System.Text.Json;
using Lette.Components;

namespace Lette.Core.JsonSerialization
{
    public class ValueConverter<T, V> : JsonConverter<T> where T : IValue<V>, new()
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<V>(ref reader, options);
            if (value == null)
                throw new Exception();
            return new() { Value = value };
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
