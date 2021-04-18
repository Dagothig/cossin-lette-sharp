using System.Text.Json.Serialization;
using System;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
    public abstract class MapConverter<T> : JsonConverter<T>
    {
        public abstract T Init();
        public abstract void ReadValue(ref Utf8JsonReader reader, ref T obj, string name, JsonSerializerOptions options);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new Exception();
            var obj = Init();
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString();
                if (!(name is string))
                    throw new Exception();
                ReadValue(ref reader, ref obj, name, options);
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new Exception();
            return obj;
        }
    }
}
